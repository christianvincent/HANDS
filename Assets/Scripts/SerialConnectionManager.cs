// SerialConnectionManager.cs (with Robust Reading Loop)
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text; // --- ADDED THIS for StringBuilder ---

public class SerialConnectionManager : MonoBehaviour
{
    [Header("Serial Connection Settings")]
    public string comPort = "COM3";
    public int baudRate = 115200;
    public int readTimeout = 100; // Note: ReadLine uses this, our new method is more resilient

    private SerialPort _serialPort;
    private Thread _serialReadThread;
    private volatile bool _isRunning = false;
    private Queue<string> _dataQueue = new Queue<string>();
    private object _queueLock = new object();

    public event Action<string> OnSerialDataReceived;

    void Awake() {}

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
            _serialPort = new SerialPort(comPort, baudRate)
            {
                ReadTimeout = this.readTimeout,
                Encoding = Encoding.UTF8 // Ensure consistent encoding
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

    // --- NEW, MORE ROBUST METHOD for reading serial data ---
    private void ReadSerialDataLoop()
    {
        StringBuilder stringBuilder = new StringBuilder();

        while (_isRunning && _serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                // Read a single byte
                int byteRead = _serialPort.ReadByte();
                if (byteRead == -1) // No data available
                {
                    continue;
                }

                // Convert byte to character
                char character = Convert.ToChar(byteRead);

                // If character is a newline, we have a complete message
                if (character == '\n')
                {
                    if (stringBuilder.Length > 0)
                    {
                        string completeLine = stringBuilder.ToString().Trim();
                        lock (_queueLock)
                        {
                            _dataQueue.Enqueue(completeLine);
                        }
                        stringBuilder.Clear(); // Reset for the next message
                    }
                }
                else
                {
                    // Append character to our message buffer
                    stringBuilder.Append(character);
                }
            }
            catch (TimeoutException) { 
                // ReadByte can also timeout, this is normal if there's no data
            }
            catch (IOException ioe) {
                Debug.LogError($"Serial port IO Exception on {comPort}: {ioe.Message}. Stopping thread.");
                _isRunning = false; 
            }
            catch (Exception e) {
                if (_isRunning) {
                    Debug.LogError($"Error reading from serial port {comPort}: {e.Message}");
                }
            }
        }
        _isRunning = false;
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