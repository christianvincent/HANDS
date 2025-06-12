// InputDataManager.cs
using UnityEngine;
using System;

public class InputDataManager : MonoBehaviour
{
    [Header("Hand Specification")]
    public HandType handType;
    // NEW: A checkbox to manually invert the final curl value if needed.
    [Tooltip("Check this if this hand's animation appears reversed (opens when it should close).")]
    public bool invertFinalCurlValue;

    [Header("Dependencies")]
    public SerialConnectionManager serialConnectionManager;

    // ... The rest of the script is the same until the ProcessReceivedData method ...

    private const int NUM_FINGERS = 5;
    private const int ADC_MAX_VALUE = 4095;
    public Quaternion TargetHandOrientation { get; private set; } = Quaternion.identity;
    public float[] PotCurlTargets { get; private set; } = new float[NUM_FINGERS];
    public int[] RawPotValues { get; private set; } = new int[NUM_FINGERS];
    private int[] _calibratedMinPot = new int[NUM_FINGERS];
    private int[] _calibratedMaxPot = new int[NUM_FINGERS];
    private bool[] _isPotCalibrated = new bool[NUM_FINGERS];
    private Quaternion _neutralOrientationOffset = Quaternion.identity;
    private bool _isOrientationOffsetSet = false;
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
            _calibratedMinPot[i] = ADC_MAX_VALUE;
            _calibratedMaxPot[i] = 0;
            _isPotCalibrated[i] = false;
        }
        LoadCalibrationSettings();
    }

    void Start()
    {
        if (serialConnectionManager == null)
        {
            Debug.LogError($"InputDataManager ({handType}): SerialConnectionManager not assigned!", this.gameObject);
            enabled = false;
            return;
        }
        serialConnectionManager.OnSerialDataReceived += ProcessReceivedData;
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
        if (!jsonString.StartsWith("{") || !jsonString.EndsWith("}"))
        {
            Debug.LogWarning($"Skipping malformed or incomplete packet for {handType}: {jsonString}");
            return;
        }

        try
        {
            Esp32CombinedData data = JsonUtility.FromJson<Esp32CombinedData>(jsonString);

            if (data.q != null && data.q.Count == 4)
            {
                float esp_w = data.q[0]; float esp_x = data.q[1]; float esp_y = data.q[2]; float esp_z = data.q[3];
                Quaternion rawMpuOrientation = new Quaternion(esp_x, esp_y, esp_z, esp_w);
                Quaternion reoriented = new Quaternion(rawMpuOrientation.x, rawMpuOrientation.z, rawMpuOrientation.y, -rawMpuOrientation.w);

                if (_isOrientationOffsetSet) { TargetHandOrientation = Quaternion.Inverse(_neutralOrientationOffset) * reoriented; }
                else { TargetHandOrientation = reoriented; }
            }

            if (data.p != null && data.p.Count >= NUM_FINGERS)
            {
                for (int i = 0; i < NUM_FINGERS; i++)
                {
                    RawPotValues[i] = data.p[i];
                    float normalizedValue = 0f;

                    if (_isPotCalibrated[i] && _calibratedMaxPot[i] != _calibratedMinPot[i])
                    {
                        normalizedValue = Mathf.InverseLerp(_calibratedMinPot[i], _calibratedMaxPot[i], RawPotValues[i]);
                    }
                    else
                    {
                        normalizedValue = (float)RawPotValues[i] / ADC_MAX_VALUE;
                    }
                    
                    // NEW: If the invert flag is checked, flip the final 0-1 value.
                    if (invertFinalCurlValue)
                    {
                        PotCurlTargets[i] = 1.0f - normalizedValue;
                    }
                    else
                    {
                        PotCurlTargets[i] = normalizedValue;
                    }

                    // The Clamp01 is implicitly handled by InverseLerp and our 1.0f - value logic.
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"InputDataManager ({handType}): Error processing JSON '{jsonString}': {e.Message}");
        }
    }

    // --- The rest of the script (calibration methods) is unchanged ---
    public void SetPotentiometerCalibrationData(int fingerIndex, int minVal, int maxVal)
    {
        if (fingerIndex < 0 || fingerIndex >= NUM_FINGERS) return;
        _calibratedMinPot[fingerIndex] = minVal;
        _calibratedMaxPot[fingerIndex] = maxVal;
        _isPotCalibrated[fingerIndex] = true; // Mark as calibrated even if min/max are same, logic handles it
    }

    public void SetNeutralOrientation(Quaternion rawNeutralOrientation)
    {
        Quaternion reoriented = new Quaternion(rawNeutralOrientation.x, rawNeutralOrientation.z, rawNeutralOrientation.y, -rawNeutralOrientation.w);
        _neutralOrientationOffset = reoriented;
        _isOrientationOffsetSet = true;
    }

    private string PrefsKey(string baseKey) => $"{handType}_{baseKey}";
    
    public void LoadCalibrationSettings()
    {
        _isOrientationOffsetSet = PlayerPrefs.GetInt(PrefsKey(KEY_IS_ORIENT_OFFSET_SET), 0) == 1;
        if (_isOrientationOffsetSet)
        {
            _neutralOrientationOffset.w = PlayerPrefs.GetFloat(PrefsKey(KEY_NEUTRAL_ORIENT_W), 1f);
            _neutralOrientationOffset.x = PlayerPrefs.GetFloat(PrefsKey(KEY_NEUTRAL_ORIENT_X), 0f);
            _neutralOrientationOffset.y = PlayerPrefs.GetFloat(PrefsKey(KEY_NEUTRAL_ORIENT_Y), 0f);
            _neutralOrientationOffset.z = PlayerPrefs.GetFloat(PrefsKey(KEY_NEUTRAL_ORIENT_Z), 0f);
        }
        for (int i = 0; i < NUM_FINGERS; i++)
        {
            _isPotCalibrated[i] = PlayerPrefs.GetInt(PrefsKey(KEY_IS_POT_CALIBRATED_PREFIX + i), 0) == 1;
            if (_isPotCalibrated[i])
            {
                _calibratedMinPot[i] = PlayerPrefs.GetInt(PrefsKey(KEY_POT_MIN_PREFIX + i), ADC_MAX_VALUE);
                _calibratedMaxPot[i] = PlayerPrefs.GetInt(PrefsKey(KEY_POT_MAX_PREFIX + i), 0);
            }
        }
    }

    public void SaveCalibrationSettings()
    {
        PlayerPrefs.SetInt(PrefsKey(KEY_IS_ORIENT_OFFSET_SET), _isOrientationOffsetSet ? 1 : 0);
        if (_isOrientationOffsetSet)
        {
            PlayerPrefs.SetFloat(PrefsKey(KEY_NEUTRAL_ORIENT_W), _neutralOrientationOffset.w);
            PlayerPrefs.SetFloat(PrefsKey(KEY_NEUTRAL_ORIENT_X), _neutralOrientationOffset.x);
            PlayerPrefs.SetFloat(PrefsKey(KEY_NEUTRAL_ORIENT_Y), _neutralOrientationOffset.y);
            PlayerPrefs.SetFloat(PrefsKey(KEY_NEUTRAL_ORIENT_Z), _neutralOrientationOffset.z);
        }
        for (int i = 0; i < NUM_FINGERS; i++)
        {
            PlayerPrefs.SetInt(PrefsKey(KEY_IS_POT_CALIBRATED_PREFIX + i), _isPotCalibrated[i] ? 1 : 0);
            if (_isPotCalibrated[i])
            {
                PlayerPrefs.SetInt(PrefsKey(KEY_POT_MIN_PREFIX + i), _calibratedMinPot[i]);
                PlayerPrefs.SetInt(PrefsKey(KEY_POT_MAX_PREFIX + i), _calibratedMaxPot[i]);
            }
        }
        PlayerPrefs.Save();
        Debug.Log($"({handType}) Calibration settings saved to PlayerPrefs.");
    }
    
    public void ClearAllPotCalibration()
    {
        for (int i = 0; i < NUM_FINGERS; i++)
        {
            _isPotCalibrated[i] = false;
            _calibratedMinPot[i] = ADC_MAX_VALUE;
            _calibratedMaxPot[i] = 0;
        }
        SaveCalibrationSettings();
    }

    public void ClearOrientationOffset()
    {
        _isOrientationOffsetSet = false;
        _neutralOrientationOffset = Quaternion.identity;
        SaveCalibrationSettings();
    }
}