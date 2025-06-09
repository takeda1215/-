using System.Text;
using UnityEngine;

// このクラスで必要になるSerialHandlerクラスは1つのゲームオブジェクトにつき1個のアタッチのため、
// LoadReceiverも1つのゲームオブジェクトにつき1個のアタッチとする.
[DisallowMultipleComponent]
[RequireComponent (typeof(SerialHandler))]
public class LoadReceiver : MonoBehaviour
{

    //========================================================================================

    [Header("変更必須設定")]
    [Tooltip("何個の数値を受信するか。\n受信文字列内の\",\"で繋がった数値の個数。")]
    [SerializeField] int _receiveValueCount = 8;

    [Tooltip("| 受信値[i] - 前回受信値[i] | の許容値。\n(0 <= i <= _receiveValueCount - 1)")]
    [SerializeField][Min(0)] float _largeNoiseValue = 450f;

    [Header("変更しなくても良い部分")]
    [Tooltip("受信値[i]の移動平均のフィルタ数。小さいノイズの除去で使用。")]
    [SerializeField][Min(1)] int _movAveFilterSize = 5;

    [Tooltip("通信開始直後は受信処理が不安定のため、実行開始～受信開始までの待機時間。")]
    [SerializeField][Min(0)] float _waitTimeBeforeReceiving = 1.0f;

    [Tooltip("各センサ値が\",\"で繋がった文字列を作成するか")]
    [SerializeField] bool _isCreateStrLoad = false;

    [Tooltip("受信文字列をfloatに変換する処理を中断するか")]
    [SerializeField] bool _isConvertProcessingAborted = false;

    [Tooltip("値処理エラーのログを表示するか")]
    [SerializeField] bool _isActiveLoadProcessError = false;

    [Header("受信値（確認用）")]
    [SerializeField] string _strLoad = "";
    [SerializeField] float[] _load;

    float[] _oldLoad;
    float[] _noisyLoad;
    float[] _denoisedLoad;
    bool[] _isFirstReceive;

    SerialHandler _serialHandler;

    MovingAverageFilter[] _movAvgFilters;

    // コンストラクタの引数は、多く見積もった時の受信文字列のバイト数.
    // 数値、コンマ、カンマのみ受信するから1文字1バイト => 受信文字列の長さと等しい.
    // センサ1個あたり15個の文字を送信すると仮定したら、17個のセンサ分受信できる.
    // それ以上の値を処理する場合、バッファサイズが2倍される.
    StringBuilder _keepNumbersAndPunctuation = new StringBuilder(256);

    //========================================================================================

    #region プロパティ.

    /// <summary>受信文字列をfloatに変換する処理を中断するか.</summary>
    public bool IsConvertProcessingAborted { get => _isConvertProcessingAborted; set => _isConvertProcessingAborted = value; }
    public int ReceiveValueCount { get => _receiveValueCount; }
    /// <summary>各センサ値が","で繋がった文字列.</summary>
    public string StrLoad { get => _strLoad; }
    public float[] Load { get => _load; }
    public float this[int index]
    {
        get => (0 <= index && index < _receiveValueCount) ? _load[index] : 0.0f;
    }

    #endregion

    //========================================================================================

    // 変数の初期化や、イベント登録処理.

    private void Start()
    {
        _serialHandler = gameObject.GetComponent<SerialHandler>();

        InitializeLoadArrays();

        // 通信開始直後が通信が不安定のため、受信を開始するまで少し待機.
        Invoke(nameof(SubscribeToDataReceivedEvent), _waitTimeBeforeReceiving);
    }

    private void OnDisable()
    {
        if (_serialHandler != null)
        {
            _serialHandler.OnDataReceived -= DataReceiveMethod;
        }
    }

    private void SubscribeToDataReceivedEvent()
    {
        if (_serialHandler == null)
        {
            PrintMsg("SerialHandlerコンポーネントの取得に失敗。", LogType.Error);
            return;
        }
        _serialHandler.OnDataReceived += DataReceiveMethod;
    }

    private void InitializeLoadArrays()
    {
        _load         = new float[_receiveValueCount];
        _oldLoad      = new float[_receiveValueCount];
        _noisyLoad    = new float[_receiveValueCount];
        _denoisedLoad = new float[_receiveValueCount];
        _isFirstReceive = new bool[_receiveValueCount];
        _movAvgFilters = new MovingAverageFilter[_receiveValueCount];

        StringBuilder initStrLoad = new StringBuilder(128);
        for (int i = 0; i < _receiveValueCount; i++)
        {
            initStrLoad.Append("0");
            if (i < _receiveValueCount - 1)
            {
                initStrLoad.Append(",");
            }
            _isFirstReceive[i] = true;
            _movAvgFilters[i] = new MovingAverageFilter(_movAveFilterSize);
        }
        _strLoad = initStrLoad.ToString();
    }

    //========================================================================================

    private void DataReceiveMethod(string receivedMsg)
    {
        if (_isConvertProcessingAborted)
        {
            return;
        }

        // 受信文字列から数値、コンマ、ピリオド、マイナスを抽出.
        _keepNumbersAndPunctuation.Clear();
        foreach (char c in receivedMsg)
        {
            if (char.IsDigit(c) || c == ',' || c == '.' || c == '-')
            {
                _keepNumbersAndPunctuation.Append(c);
            }
        }
        string OnlyNumbersAndPunctuationMsg = _keepNumbersAndPunctuation.ToString();

        // 受信した文字列が途中で途切れていないか確認.
        string[] splitedMsg = OnlyNumbersAndPunctuationMsg.Split(',');
        if (splitedMsg.Length != _receiveValueCount)
        {
            PrintMsg($"{_receiveValueCount}個の値を受信するところ{splitedMsg.Length}個の値を受信した。\n" +
                     $"数値と句読点の文字列:{OnlyNumbersAndPunctuationMsg}",
                     LogType.Warning);
            return;
        }

        // 受信した数値の妥当性を確認.
        for (int i = 0; i < _receiveValueCount; i++)
        {
            // 文字列をfloatに変換.
            if (float.TryParse(splitedMsg[i], out float convertedValue))
            {
                _noisyLoad[i] = convertedValue;
            }
            else
            {
                PrintMsg($"数値に変換できない箇所がある。\n" +
                         $"受信文字列:{OnlyNumbersAndPunctuationMsg}", 
                         LogType.Warning);
                continue;
            }

            // 初めて値を受信したかを知る理由は、load-oldLoad の許容値を考慮するため.
            // センサのデフォルト値が1000で差の許容値が500の場合、
            // 最初に受信した時の差は 1000-0=1000 となるため、大きいノイズとして処理される.
            // よって、初めて値を受信した時はloadとoldLoadを等しくすることで、この問題を回避する.
            if (_isFirstReceive[i])
            {
                _isFirstReceive[i] = false;
                _oldLoad[i] = _noisyLoad[i];
            }

            // 大きいノイズ除去.
            // 小さいノイズ除去は移動"平均"を計算するため、平均値を乱す外れ値(大きいノイズ)を先に除去する.
            float deltaAbsLoad = Mathf.Abs(_noisyLoad[i] - _oldLoad[i]);
            if (deltaAbsLoad < _largeNoiseValue)
            {
                _denoisedLoad[i] = _noisyLoad[i];
            }
            else
            {
                PrintMsg($"{i}番目の大きいノイズを除去。\n" +
                         $"{_oldLoad[i]} => {_noisyLoad[i]} : delta = {deltaAbsLoad}", 
                         LogType.Warning);
                _denoisedLoad[i] = _oldLoad[i];
            }

            // 小さいノイズ除去.
            _denoisedLoad[i] = _movAvgFilters[i].ApplyFilter(_denoisedLoad[i]);

            // 値を更新.
            _oldLoad[i] = _load[i] = _denoisedLoad[i];
        }

        // 必要があれば各センサ値が","で繋がった文字列を作成.
        if (_isCreateStrLoad)
        {
            _strLoad = string.Join(",", _load);
        }
    }

    //========================================================================================

    #region 確認用のデバッグ関数.

    private void PrintMsg(string msg, LogType type)
    {
        if (!_isActiveLoadProcessError)
        {
            return;
        }

        switch (type)
        {
            case LogType.Log:
                Debug.Log(msg);
                break;

            case LogType.Warning:
                Debug.LogWarning(msg);
                break;

            case LogType.Error:
                Debug.LogError(msg);
                break;

            default:
                Debug.Log(msg);
                break;
        }
    }

    #endregion

    //========================================================================================

}
