// SerialConnectionManager.cs
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;
using System.Collections.Generic; // For Queue<>
using System.IO;                 // --- ADDED THIS for IOException ---

public class SerialConnectionManager : MonoBehaviour
{
    [Header("Serial Connection Settings")]
    public string comPort = "COM3";
    public int baudRate = 115200;
    public int readTimeout = 100;

    private SerialPort _serialPort;
    private Thread _serialReadThread;
    private volatile bool _isRunning = false;
    private Queue<string> _dataQueue = new Queue<string>();
    private object _queueLock = new object();

    public event Action<string> OnSerialDataReceived;

    void Awake() {} // Awake can be empty if not used

    void Start()
    {
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
            _serialPort = new SerialPort(comPort, baudRate) { ReadTimeout = this.readTimeout };
            _serialPort.Open();
            _isRunning = true;
            _serialReadThread = new Thread(ReadSerialDataLoop) { IsBackground = true };
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
        while (_isRunning && _serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                string line = _serialPort.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    lock (_queueLock) { _dataQueue.Enqueue(line.Trim()); }
                }
            }
            catch (TimeoutException) { /* Expected, continue */ }
            catch (IOException ioe) // This requires System.IO
            {
                Debug.LogError($"Serial port IO Exception on {comPort}: {ioe.Message}. Attempting to stop thread.");
                _isRunning = false; // Signal loop to stop
            }
            catch (Exception e)
            {
                if (_isRunning) { Debug.LogError($"Error reading from serial port {comPort}: {e.Message}"); }
            }
        }
        _isRunning = false; // Ensure isRunning is false if loop exits
        Debug.Log($"Serial read thread for {comPort} has finished.");
    }

    void Update()
    {
        List<string> linesToProcess = new List<string>();
        lock (_queueLock)
        {
            while (_dataQueue.Count > 0) { linesToProcess.Add(_dataQueue.Dequeue()); }
        }
        foreach(string dataLine in linesToProcess)
        {
            OnSerialDataReceived?.Invoke(dataLine);
        }
    }

    public void CloseConnection()
    {
        _isRunning = false;
        if (_serialReadThread != null && _serialReadThread.IsAlive)
        {
            if (!_serialReadThread.Join(500)) {
                Debug.LogWarning($"Serial read thread for {comPort} did not finish in time.");
            }
            _serialReadThread = null;
        }
        if (_serialPort != null)
        {
            if (_serialPort.IsOpen) {
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