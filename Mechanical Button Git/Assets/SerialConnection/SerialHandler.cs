// �V���A���ʐM�́A�����̃C���X�^���X���瓯��COM�ԍ��̃|�[�g�֐ڑ����o���Ȃ�.
// ���̂��߁A���̃N���X�̓V���A���ʐM�̑���M���s��.

using System;
// "System.IO.Ports"��Unity�̃v���W�F�N�g�ňȉ��̕ύX���K�v.
// File > Build Settings > Player Settings > Other Settings >
// API Compatibility Level* > .NET Framework
using System.IO.Ports;
using System.Threading;
using UnityEngine;

// 1�̃Q�[���I�u�W�F�N�g��SerialHandler�R���|�[�l���g�𕡐��A�^�b�`�����ꍇ�A
// GetComponent<SerialHandler>�Ŏ擾����l�͂ǂ�COM�ԍ��̃N���X��������Ȃ����߁A
// 1�̃Q�[���I�u�W�F�N�g�ɂ�1�̃A�^�b�`�Ƃ���.
[DisallowMultipleComponent]
public class SerialHandler : MonoBehaviour
{
    public event Action<string> OnDataReceived;

    [SerializeField] string _portName = "COM1";
    [SerializeField] int _baudRate = 115200;

    private SerialPort _serialPort;
    private Thread _receiveThread;
    private bool _isOpenedReceiveThread = false;

    [Header("��M�l�i�m�F�p�j")]
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
            string errorMessage = $"{(_portName == "" ? $"{gameObject.name}��COM�ԍ������ݒ�" : $"{_portName}�ŃV���A���ʐM�̊J�n�Ɏ��s")} : {e.Message}";
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
                    Debug.LogError($"{_portName}�ŃV���A���ʐM�̎�M�G���[ : {e.Message}");
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
            Debug.LogWarning($"{_portName}�ŃV���A���ʐM�̑��M�G���[ : {e.Message}");
        }
    }

    //=========================================================================================

}
