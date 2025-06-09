using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialReceiver : MonoBehaviour
{
    public string portName = "COM4";
    public int baudRate = 115200;
    public float offsetValue = 0.0f;  // Unity エディターで変更可能な補正値

    private SerialPort serialPort;
    private Thread thread;
    private bool isRunning = false;

    public string receivedData = "";  // 受信データを保存するパブリック変数

    void Awake()
    {
        OpenSerialPort();
    }

    void OnDestroy()
    {
        CloseSerialPort();
    }

    private void OpenSerialPort()
    {
        serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        serialPort.Open();
        serialPort.ReadTimeout = 20;

        isRunning = true;
        thread = new Thread(ReadData);
        thread.Start();
    }

    private void CloseSerialPort()
    {
        isRunning = false;
        if (thread != null && thread.IsAlive)
        {
            thread.Join();
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            serialPort.Dispose();
        }
    }

    private void ReadData()
    {
        while (isRunning && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                if (serialPort.BytesToRead > 0)
                {
                    receivedData = serialPort.ReadLine();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Failed to read from serial port: " + ex.Message);
            }
        }
    }
}
