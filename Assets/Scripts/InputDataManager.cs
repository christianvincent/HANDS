// InputDataManager.cs
using UnityEngine;
using System;
using System.Collections.Generic; // For List (used in Payloads)

public class InputDataManager : MonoBehaviour
{
    [Header("Dependencies")]
    public SerialConnectionManager serialConnectionManager;

    [Header("Configuration")]
    private const int NUM_FINGERS = 5;
    private int adcMaxValue = 4095; // ESP32 default ADC max value (12-bit)

    // Public properties to provide the latest data
    public Quaternion TargetHandOrientation { get; private set; } = Quaternion.identity;
    public float[] PotCurlTargets { get; private set; } = new float[NUM_FINGERS];
    public int[] RawPotValues { get; private set; } = new int[NUM_FINGERS];

    // Calibration data
    private int[] _calibratedMinPot = new int[NUM_FINGERS];
    private int[] _calibratedMaxPot = new int[NUM_FINGERS];
    private bool[] _isPotCalibrated = new bool[NUM_FINGERS];
    private Quaternion _neutralOrientationOffset = Quaternion.identity;
    private bool _isOrientationOffsetSet = false;

    // PlayerPrefs keys
    private const string KEY_POT_MIN_PREFIX = "PotMin_";
    private const string KEY_POT_MAX_PREFIX = "PotMax_";
    private const string KEY_IS_POT_CALIBRATED_PREFIX = "IsPotCalibrated_";
    private const string KEY_NEUTRAL_ORIENT_W = "NeutralOrientW";
    private const string KEY_NEUTRAL_ORIENT_X = "NeutralOrientX";
    private const string KEY_NEUTRAL_ORIENT_Y = "NeutralOrientY";
    private const string KEY_NEUTRAL_ORIENT_Z = "NeutralOrientZ";
    private const string KEY_IS_ORIENT_OFFSET_SET = "IsOrientOffsetSet";

    void Awake()
    {
        for (int i = 0; i < NUM_FINGERS; i++)
        {
            PotCurlTargets[i] = 0f;
            RawPotValues[i] = 0;
            // Default uncalibrated values (min is high, max is low to force update on first calibration)
            _calibratedMinPot[i] = adcMaxValue;
            _calibratedMaxPot[i] = 0;
            _isPotCalibrated[i] = false;
        }
        LoadCalibrationSettings(); // Load any saved settings
    }

    void Start()
    {
        if (serialConnectionManager == null)
        {
            Debug.LogError("InputDataManager: SerialConnectionManager not assigned!");
            enabled = false; return;
        }
        serialConnectionManager.OnSerialDataReceived += ProcessReceivedData;
        Debug.Log("InputDataManager subscribed to OnSerialDataReceived.");
    }

    void OnDestroy()
    {
        if (serialConnectionManager != null)
        {
            serialConnectionManager.OnSerialDataReceived -= ProcessReceivedData;
        }
    }

    private void ProcessReceivedData(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString)) return;

        try
        {
            Esp32JsonData baseData = JsonUtility.FromJson<Esp32JsonData>(jsonString);

            if (baseData.key == "/orientation")
            {
                OrientationPayload orientationData = JsonUtility.FromJson<OrientationPayload>(jsonString);
                if (orientationData.value != null && orientationData.value.Count == 4)
                {
                    float esp_w = orientationData.value[0]; float esp_x = orientationData.value[1];
                    float esp_y = orientationData.value[2]; float esp_z = orientationData.value[3];
                    Quaternion rawMpuOrientation = new Quaternion(esp_x, esp_y, esp_z, esp_w);

                    if (_isOrientationOffsetSet)
                    {
                        TargetHandOrientation = Quaternion.Inverse(_neutralOrientationOffset) * rawMpuOrientation;
                    }
                    else
                    {
                        TargetHandOrientation = rawMpuOrientation;
                    }
                }
            }
            else if (baseData.key == "/pots")
            {
                PotsPayload potsData = JsonUtility.FromJson<PotsPayload>(jsonString);
                if (potsData.value != null)
                {
                    for (int i = 0; i < potsData.value.Count && i < NUM_FINGERS; i++)
                    {
                        RawPotValues[i] = potsData.value[i]; // Store raw value
                        if (_isPotCalibrated[i] && _calibratedMaxPot[i] > _calibratedMinPot[i])
                        {
                            PotCurlTargets[i] = Mathf.Clamp01(Mathf.InverseLerp(_calibratedMinPot[i], _calibratedMaxPot[i], RawPotValues[i]));
                        }
                        else // Use default mapping if not calibrated or if range is invalid
                        {
                            PotCurlTargets[i] = Mathf.Clamp01((float)RawPotValues[i] / adcMaxValue);
                        }
                    }
                }
            }
        }
        catch (Exception e) { Debug.LogError($"InputDataManager: Error processing JSON '{jsonString}': {e.Message}"); }
    }

    // --- Calibration Methods ---
    public void SetPotentiometerCalibrationData(int fingerIndex, int minVal, int maxVal)
    {
        if (fingerIndex < 0 || fingerIndex >= NUM_FINGERS) return;
        _calibratedMinPot[fingerIndex] = minVal;
        _calibratedMaxPot[fingerIndex] = maxVal;
        _isPotCalibrated[fingerIndex] = (maxVal > minVal); // Consider calibrated if range is valid
        Debug.Log($"Potentiometer calibration set for finger {fingerIndex}: Min={minVal}, Max={maxVal}, Calibrated={_isPotCalibrated[fingerIndex]}");
    }

    public void SetNeutralOrientation(Quaternion neutralOrientation)
    {
        _neutralOrientationOffset = neutralOrientation;
        _isOrientationOffsetSet = true;
        Debug.Log($"Neutral orientation offset set: {neutralOrientation.eulerAngles}");
    }
    
    public void ClearPotCalibrationForFinger(int fingerIndex)
    {
        if (fingerIndex < 0 || fingerIndex >= NUM_FINGERS) return;
        _calibratedMinPot[fingerIndex] = adcMaxValue;
        _calibratedMaxPot[fingerIndex] = 0;
        _isPotCalibrated[fingerIndex] = false;
        PlayerPrefs.SetInt(KEY_IS_POT_CALIBRATED_PREFIX + fingerIndex, 0); // Mark as not calibrated
        Debug.Log($"Potentiometer calibration cleared for finger {fingerIndex}");
        SaveCalibrationSettings(); // Save the cleared state
    }

    public void ClearAllPotCalibration()
    {
        for (int i = 0; i < NUM_FINGERS; i++)
        {
            _calibratedMinPot[i] = adcMaxValue;
            _calibratedMaxPot[i] = 0;
            _isPotCalibrated[i] = false;
            PlayerPrefs.SetInt(KEY_IS_POT_CALIBRATED_PREFIX + i, 0);
        }
        Debug.Log("All potentiometer calibrations cleared.");
        SaveCalibrationSettings();
    }

    public void ClearOrientationOffset()
    {
        _neutralOrientationOffset = Quaternion.identity;
        _isOrientationOffsetSet = false;
        PlayerPrefs.SetInt(KEY_IS_ORIENT_OFFSET_SET, 0);
        Debug.Log("Neutral orientation offset cleared.");
        SaveCalibrationSettings();
    }

    public void LoadCalibrationSettings()
    {
        for (int i = 0; i < NUM_FINGERS; i++)
        {
            _isPotCalibrated[i] = PlayerPrefs.GetInt(KEY_IS_POT_CALIBRATED_PREFIX + i, 0) == 1;
            if (_isPotCalibrated[i])
            {
                _calibratedMinPot[i] = PlayerPrefs.GetInt(KEY_POT_MIN_PREFIX + i, adcMaxValue);
                _calibratedMaxPot[i] = PlayerPrefs.GetInt(KEY_POT_MAX_PREFIX + i, 0);
            }
        }

        _isOrientationOffsetSet = PlayerPrefs.GetInt(KEY_IS_ORIENT_OFFSET_SET, 0) == 1;
        if (_isOrientationOffsetSet)
        {
            _neutralOrientationOffset.w = PlayerPrefs.GetFloat(KEY_NEUTRAL_ORIENT_W, 1f);
            _neutralOrientationOffset.x = PlayerPrefs.GetFloat(KEY_NEUTRAL_ORIENT_X, 0f);
            _neutralOrientationOffset.y = PlayerPrefs.GetFloat(KEY_NEUTRAL_ORIENT_Y, 0f);
            _neutralOrientationOffset.z = PlayerPrefs.GetFloat(KEY_NEUTRAL_ORIENT_Z, 0f);
        }
        Debug.Log("InputDataManager: Calibration settings loaded from PlayerPrefs.");
    }

    public void SaveCalibrationSettings()
    {
        for (int i = 0; i < NUM_FINGERS; i++)
        {
            PlayerPrefs.SetInt(KEY_IS_POT_CALIBRATED_PREFIX + i, _isPotCalibrated[i] ? 1 : 0);
            if (_isPotCalibrated[i])
            {
                PlayerPrefs.SetInt(KEY_POT_MIN_PREFIX + i, _calibratedMinPot[i]);
                PlayerPrefs.SetInt(KEY_POT_MAX_PREFIX + i, _calibratedMaxPot[i]);
            }
        }

        PlayerPrefs.SetInt(KEY_IS_ORIENT_OFFSET_SET, _isOrientationOffsetSet ? 1 : 0);
        if (_isOrientationOffsetSet)
        {
            PlayerPrefs.SetFloat(KEY_NEUTRAL_ORIENT_W, _neutralOrientationOffset.w);
            PlayerPrefs.SetFloat(KEY_NEUTRAL_ORIENT_X, _neutralOrientationOffset.x);
            PlayerPrefs.SetFloat(KEY_NEUTRAL_ORIENT_Y, _neutralOrientationOffset.y);
            PlayerPrefs.SetFloat(KEY_NEUTRAL_ORIENT_Z, _neutralOrientationOffset.z);
        }
        PlayerPrefs.Save(); // Important: Write PlayerPrefs to disk
        Debug.Log("InputDataManager: Calibration settings saved to PlayerPrefs.");
    }
}