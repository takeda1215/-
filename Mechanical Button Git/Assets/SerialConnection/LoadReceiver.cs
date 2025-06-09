using System.Text;
using UnityEngine;

// ���̃N���X�ŕK�v�ɂȂ�SerialHandler�N���X��1�̃Q�[���I�u�W�F�N�g�ɂ�1�̃A�^�b�`�̂��߁A
// LoadReceiver��1�̃Q�[���I�u�W�F�N�g�ɂ�1�̃A�^�b�`�Ƃ���.
[DisallowMultipleComponent]
[RequireComponent (typeof(SerialHandler))]
public class LoadReceiver : MonoBehaviour
{

    //========================================================================================

    [Header("�ύX�K�{�ݒ�")]
    [Tooltip("���̐��l����M���邩�B\n��M���������\",\"�Ōq���������l�̌��B")]
    [SerializeField] int _receiveValueCount = 8;

    [Tooltip("| ��M�l[i] - �O���M�l[i] | �̋��e�l�B\n(0 <= i <= _receiveValueCount - 1)")]
    [SerializeField][Min(0)] float _largeNoiseValue = 450f;

    [Header("�ύX���Ȃ��Ă��ǂ�����")]
    [Tooltip("��M�l[i]�̈ړ����ς̃t�B���^���B�������m�C�Y�̏����Ŏg�p�B")]
    [SerializeField][Min(1)] int _movAveFilterSize = 5;

    [Tooltip("�ʐM�J�n����͎�M�������s����̂��߁A���s�J�n�`��M�J�n�܂ł̑ҋ@���ԁB")]
    [SerializeField][Min(0)] float _waitTimeBeforeReceiving = 1.0f;

    [Tooltip("�e�Z���T�l��\",\"�Ōq��������������쐬���邩")]
    [SerializeField] bool _isCreateStrLoad = false;

    [Tooltip("��M�������float�ɕϊ����鏈���𒆒f���邩")]
    [SerializeField] bool _isConvertProcessingAborted = false;

    [Tooltip("�l�����G���[�̃��O��\�����邩")]
    [SerializeField] bool _isActiveLoadProcessError = false;

    [Header("��M�l�i�m�F�p�j")]
    [SerializeField] string _strLoad = "";
    [SerializeField] float[] _load;

    float[] _oldLoad;
    float[] _noisyLoad;
    float[] _denoisedLoad;
    bool[] _isFirstReceive;

    SerialHandler _serialHandler;

    MovingAverageFilter[] _movAvgFilters;

    // �R���X�g���N�^�̈����́A�������ς��������̎�M������̃o�C�g��.
    // ���l�A�R���}�A�J���}�̂ݎ�M���邩��1����1�o�C�g => ��M������̒����Ɠ�����.
    // �Z���T1������15�̕����𑗐M����Ɖ��肵����A17�̃Z���T����M�ł���.
    // ����ȏ�̒l����������ꍇ�A�o�b�t�@�T�C�Y��2�{�����.
    StringBuilder _keepNumbersAndPunctuation = new StringBuilder(256);

    //========================================================================================

    #region �v���p�e�B.

    /// <summary>��M�������float�ɕϊ����鏈���𒆒f���邩.</summary>
    public bool IsConvertProcessingAborted { get => _isConvertProcessingAborted; set => _isConvertProcessingAborted = value; }
    public int ReceiveValueCount { get => _receiveValueCount; }
    /// <summary>�e�Z���T�l��","�Ōq������������.</summary>
    public string StrLoad { get => _strLoad; }
    public float[] Load { get => _load; }
    public float this[int index]
    {
        get => (0 <= index && index < _receiveValueCount) ? _load[index] : 0.0f;
    }

    #endregion

    //========================================================================================

    // �ϐ��̏�������A�C�x���g�o�^����.

    private void Start()
    {
        _serialHandler = gameObject.GetComponent<SerialHandler>();

        InitializeLoadArrays();

        // �ʐM�J�n���オ�ʐM���s����̂��߁A��M���J�n����܂ŏ����ҋ@.
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
            PrintMsg("SerialHandler�R���|�[�l���g�̎擾�Ɏ��s�B", LogType.Error);
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

        // ��M�����񂩂琔�l�A�R���}�A�s���I�h�A�}�C�i�X�𒊏o.
        _keepNumbersAndPunctuation.Clear();
        foreach (char c in receivedMsg)
        {
            if (char.IsDigit(c) || c == ',' || c == '.' || c == '-')
            {
                _keepNumbersAndPunctuation.Append(c);
            }
        }
        string OnlyNumbersAndPunctuationMsg = _keepNumbersAndPunctuation.ToString();

        // ��M���������񂪓r���œr�؂�Ă��Ȃ����m�F.
        string[] splitedMsg = OnlyNumbersAndPunctuationMsg.Split(',');
        if (splitedMsg.Length != _receiveValueCount)
        {
            PrintMsg($"{_receiveValueCount}�̒l����M����Ƃ���{splitedMsg.Length}�̒l����M�����B\n" +
                     $"���l�Ƌ�Ǔ_�̕�����:{OnlyNumbersAndPunctuationMsg}",
                     LogType.Warning);
            return;
        }

        // ��M�������l�̑Ó������m�F.
        for (int i = 0; i < _receiveValueCount; i++)
        {
            // �������float�ɕϊ�.
            if (float.TryParse(splitedMsg[i], out float convertedValue))
            {
                _noisyLoad[i] = convertedValue;
            }
            else
            {
                PrintMsg($"���l�ɕϊ��ł��Ȃ��ӏ�������B\n" +
                         $"��M������:{OnlyNumbersAndPunctuationMsg}", 
                         LogType.Warning);
                continue;
            }

            // ���߂Ēl����M��������m�闝�R�́Aload-oldLoad �̋��e�l���l�����邽��.
            // �Z���T�̃f�t�H���g�l��1000�ō��̋��e�l��500�̏ꍇ�A
            // �ŏ��Ɏ�M�������̍��� 1000-0=1000 �ƂȂ邽�߁A�傫���m�C�Y�Ƃ��ď��������.
            // ����āA���߂Ēl����M��������load��oldLoad�𓙂������邱�ƂŁA���̖����������.
            if (_isFirstReceive[i])
            {
                _isFirstReceive[i] = false;
                _oldLoad[i] = _noisyLoad[i];
            }

            // �傫���m�C�Y����.
            // �������m�C�Y�����͈ړ�"����"���v�Z���邽�߁A���ϒl�𗐂��O��l(�傫���m�C�Y)���ɏ�������.
            float deltaAbsLoad = Mathf.Abs(_noisyLoad[i] - _oldLoad[i]);
            if (deltaAbsLoad < _largeNoiseValue)
            {
                _denoisedLoad[i] = _noisyLoad[i];
            }
            else
            {
                PrintMsg($"{i}�Ԗڂ̑傫���m�C�Y�������B\n" +
                         $"{_oldLoad[i]} => {_noisyLoad[i]} : delta = {deltaAbsLoad}", 
                         LogType.Warning);
                _denoisedLoad[i] = _oldLoad[i];
            }

            // �������m�C�Y����.
            _denoisedLoad[i] = _movAvgFilters[i].ApplyFilter(_denoisedLoad[i]);

            // �l���X�V.
            _oldLoad[i] = _load[i] = _denoisedLoad[i];
        }

        // �K�v������Ίe�Z���T�l��","�Ōq��������������쐬.
        if (_isCreateStrLoad)
        {
            _strLoad = string.Join(",", _load);
        }
    }

    //========================================================================================

    #region �m�F�p�̃f�o�b�O�֐�.

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
