// SerialConnectionManager.cs (Simplified Inspector-Driven Version)
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Text;

public class SerialConnectionManager : MonoBehaviour
{
    [Header("Serial Connection Settings")]
    [Tooltip("Set the COM Port for this device here (e.g., COM3).")]
    public string comPort = "COM3";
    public int baudRate = 115200;
    public int readTimeout = 100;

    // Public property to safely check connection status from other scripts
    public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

    // Private variables for handling the connection and threading
    private SerialPort _serialPort;
    private Thread _serialReadThread;
    private volatile bool _isRunning = false;
    private readonly Queue<string> _dataQueue = new Queue<string>();
    private readonly object _queueLock = new object();

    // Event that other scripts (like InputDataManager) can subscribe to
    public event Action<string> OnSerialDataReceived;

    // When the script starts, it immediately tries to connect.
    void Start()
    {
        if (string.IsNullOrEmpty(comPort))
        {
            Debug.LogError("COM Port is not set in the Inspector. Connection failed.");
            return;
        }
        Connect();
    }

    public void Connect()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            Debug.LogWarning($"Serial port {comPort} is already open.");
            return;
        }
        try
        {
            _serialPort = new SerialPort(comPort, baudRate)
            {
                ReadTimeout = this.readTimeout,
                Encoding = Encoding.UTF8
            };
            _serialPort.Open();
            _isRunning = true;
            _serialReadThread = new Thread(ReadSerialDataLoop)
            {
                IsBackground = true
            };
            _serialReadThread.Start();
            Debug.Log($"Successfully opened serial port {comPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error opening serial port {comPort}: {e.Message}");
            _isRunning = false;
        }
    }

    private void ReadSerialDataLoop()
    {
        StringBuilder stringBuilder = new StringBuilder();
        while (_isRunning && _serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                int byteRead = _serialPort.ReadByte();
                if (byteRead == -1) continue;
                char character = Convert.ToChar(byteRead);
                if (character == '\n')
                {
                    if (stringBuilder.Length > 0)
                    {
                        string completeLine = stringBuilder.ToString().Trim();
                        lock (_queueLock) { _dataQueue.Enqueue(completeLine); }
                        stringBuilder.Clear();
                    }
                }
                else { stringBuilder.Append(character); }
            }
            catch (TimeoutException) { }
            catch (Exception e)
            {
                if (_isRunning) { Debug.LogError($"Error reading from serial port {comPort}: {e.Message}"); }
            }
        }
    }

    void Update()
    {
        if (_dataQueue.Count > 0)
        {
            lock (_queueLock)
            {
                while (_dataQueue.Count > 0)
                {
                    OnSerialDataReceived?.Invoke(_dataQueue.Dequeue());
                }
            }
        }
    }

    public void CloseConnection()
    {
        _isRunning = false;
        if (_serialReadThread != null && _serialReadThread.IsAlive)
        {
            if (!_serialReadThread.Join(500)) { Debug.LogWarning($"Serial read thread for {comPort} did not finish in time."); }
        }
        if (_serialPort != null)
        {
            if (_serialPort.IsOpen)
            {
                try { _serialPort.Close(); Debug.Log($"Serial port {comPort} explicitly closed."); }
                catch (Exception e) { Debug.LogError($"Error closing serial port {comPort}: {e.Message}"); }
            }
            _serialPort.Dispose();
            _serialPort = null;
        }
    }

    void OnApplicationQuit() { CloseConnection(); }
    void OnDestroy() { CloseConnection(); }
}