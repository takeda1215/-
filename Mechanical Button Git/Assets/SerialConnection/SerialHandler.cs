// シリアル通信は、複数のインスタンスから同じCOM番号のポートへ接続が出来ない.
// そのため、このクラスはシリアル通信の送受信を行う.

using System;
// "System.IO.Ports"はUnityのプロジェクトで以下の変更が必要.
// File > Build Settings > Player Settings > Other Settings >
// API Compatibility Level* > .NET Framework
using System.IO.Ports;
using System.Threading;
using UnityEngine;

// 1つのゲームオブジェクトにSerialHandlerコンポーネントを複数アタッチした場合、
// GetComponent<SerialHandler>で取得する値はどのCOM番号のクラスか分からないため、
// 1つのゲームオブジェクトにつき1つのアタッチとする.
[DisallowMultipleComponent]
public class SerialHandler : MonoBehaviour
{
    public event Action<string> OnDataReceived;

    [SerializeField] string _portName = "COM1";
    [SerializeField] int _baudRate = 115200;

    private SerialPort _serialPort;
    private Thread _receiveThread;
    private bool _isOpenedReceiveThread = false;

    [Header("受信値（確認用）")]
    [SerializeField] string _message;
    private bool _isReceivedNewMessage = false;

    //=========================================================================================

    private void Awake()
    {
        Open();
    }

    private void Update()
    {
        if (!_isReceivedNewMessage)
        {
            return;
        }

        OnDataReceived?.Invoke(_message);
        _isReceivedNewMessage = false;
    }

    private void OnDestroy()
    {
        Close();
    }

    //=========================================================================================

    private void Open()
    {
        try
        {
            _serialPort = new SerialPort(_portName, _baudRate);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 1000;
            _serialPort.Open();
        }
        catch (Exception e)
        {
            string errorMessage = $"{(_portName == "" ? $"{gameObject.name}のCOM番号が未設定" : $"{_portName}でシリアル通信の開始に失敗")} : {e.Message}";
            Debug.LogError(errorMessage);
            return;
        }

        _isOpenedReceiveThread = true;
        _receiveThread = new Thread(Read);
        _receiveThread.Start();
    }

    private void Close()
    {
        _isReceivedNewMessage = false;
        _isOpenedReceiveThread = false;

        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
        }

        if (_receiveThread != null && _receiveThread.IsAlive)
        {
            _receiveThread.Join();
        }
    }

    //=========================================================================================

    private void Read()
    {
        while (_isOpenedReceiveThread && _serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                _message = _serialPort.ReadLine();
                _isReceivedNewMessage = true;
            }
            catch (Exception e)
            {
                if (_isOpenedReceiveThread)
                {
                    Debug.LogError($"{_portName}でシリアル通信の受信エラー : {e.Message}");
                }
            }
        }
    }

    public void Write(string message)
    {
        try
        {
            _serialPort.Write(message);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"{_portName}でシリアル通信の送信エラー : {e.Message}");
        }
    }

    //=========================================================================================

}
