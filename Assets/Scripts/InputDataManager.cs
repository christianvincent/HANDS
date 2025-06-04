// InputDataManager.cs
using UnityEngine;
using System; // For Action
using System.Collections.Generic; // For List (used in Payloads)

// Note: Esp32JsonData, OrientationPayload, PotsPayload should be defined in your DataTypes.cs

public class InputDataManager : MonoBehaviour
{
    [Header("Dependencies")]
    public SerialConnectionManager serialConnectionManager; // Assign in Inspector

    [Header("Configuration")]
    private int adcMaxValue = 4095; // ESP32 default ADC max value (12-bit)

    // Public properties to provide the latest processed data
    public Quaternion TargetHandOrientation { get; private set; } = Quaternion.identity;
    public float[] PotCurlTargets { get; private set; } = new float[5]; // Assuming 5 potentiometers

    void Awake()
    {
        // Initialize PotCurlTargets to avoid null issues if accessed before first data
        for (int i = 0; i < PotCurlTargets.Length; i++)
        {
            PotCurlTargets[i] = 0f;
        }
    }

    void Start()
    {
        if (serialConnectionManager == null)
        {
            Debug.LogError("InputDataManager: SerialConnectionManager not assigned in Inspector!");
            enabled = false; // Disable this script if dependency is missing
            return;
        }
        // Subscribe to the event from SerialConnectionManager
        serialConnectionManager.OnSerialDataReceived += ProcessReceivedData;
        Debug.Log("InputDataManager subscribed to OnSerialDataReceived.");
    }

    void OnDestroy()
    {
        // Unsubscribe when this object is destroyed to prevent memory leaks
        if (serialConnectionManager != null)
        {
            serialConnectionManager.OnSerialDataReceived -= ProcessReceivedData;
            Debug.Log("InputDataManager unsubscribed from OnSerialDataReceived.");
        }
    }

    private void ProcessReceivedData(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return;
        }

        try
        {
            // First, parse as a base message to check the key
            // Ensure Esp32JsonData is defined in DataTypes.cs
            Esp32JsonData baseData = JsonUtility.FromJson<Esp32JsonData>(jsonString);

            if (baseData.key == "/orientation")
            {
                // Ensure OrientationPayload is defined in DataTypes.cs
                OrientationPayload orientationData = JsonUtility.FromJson<OrientationPayload>(jsonString);
                if (orientationData.value != null && orientationData.value.Count == 4)
                {
                    // ESP32 sends: w, x, y, z
                    // Unity Quaternion constructor is: x, y, z, w
                    float esp_w = orientationData.value[0];
                    float esp_x = orientationData.value[1];
                    float esp_y = orientationData.value[2];
                    float esp_z = orientationData.value[3];
                    
                    // CRITICAL: This needs to be adjusted for your MPU6050's mounting & coordinate system
                    TargetHandOrientation = new Quaternion(esp_x, esp_y, esp_z, esp_w); 
                    // Example of a common adjustment: new Quaternion(-esp_y, esp_z, esp_x, esp_w);
                }
            }
            else if (baseData.key == "/pots")
            {
                // Ensure PotsPayload is defined in DataTypes.cs
                PotsPayload potsData = JsonUtility.FromJson<PotsPayload>(jsonString);
                if (potsData.value != null)
                {
                    for (int i = 0; i < potsData.value.Count && i < PotCurlTargets.Length; i++)
                    {
                        PotCurlTargets[i] = Mathf.Clamp01((float)potsData.value[i] / adcMaxValue);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"InputDataManager: Error processing JSON data '{jsonString}': {e.Message}");
        }
    }
}