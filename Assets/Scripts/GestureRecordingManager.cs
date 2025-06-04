// GestureRecordingManager.cs
using UnityEngine;
using TMPro; // For TextMeshPro elements
using System;
using System.Collections.Generic; // For List

// Note: GestureData and GestureFrame structs/classes should be defined in your DataTypes.cs file

public class GestureRecordingManager : MonoBehaviour
{
    [Header("Dependencies")]
    public InputDataManager inputDataManager;         // Assign your InputDataManager
    public GestureStorageManager gestureStorageManager; // Assign your GestureStorageManager

    [Header("Recording UI Elements")]
    public TMP_InputField gestureNameInputField;      // Assign your gesture name InputField
    public TMP_Text recordingStatusText;          // Assign your status Text element

    [Header("Recording Configuration")]
    public float trimDurationStartSec = 1.0f;
    public float trimDurationEndSec = 1.0f;

    // Recording state
    private bool _isRecording = false;
    private GestureData _currentGestureData;
    private float _recordingStartTime;

    void Start()
    {
        if (inputDataManager == null)
        {
            Debug.LogError("GestureRecordingManager: InputDataManager not assigned!");
            enabled = false; return;
        }
        if (gestureStorageManager == null)
        {
            Debug.LogError("GestureRecordingManager: GestureStorageManager not assigned!");
            enabled = false; return;
        }
        if (gestureNameInputField == null) Debug.LogWarning("GestureRecordingManager: Gesture Name InputField not assigned.");
        if (recordingStatusText != null) recordingStatusText.text = "Ready to Record";
    }

    void Update()
    {
        if (!_isRecording || _currentGestureData == null || inputDataManager == null)
        {
            return;
        }

        // Capture frame data during recording
        float currentTime = Time.time - _recordingStartTime;
        Quaternion currentOrientation = inputDataManager.TargetHandOrientation;
        
        // Ensure we create a new array instance for finger curls for this frame
        float[] currentPotCurls = new float[inputDataManager.PotCurlTargets.Length];
        Array.Copy(inputDataManager.PotCurlTargets, currentPotCurls, inputDataManager.PotCurlTargets.Length);
        
        _currentGestureData.frames.Add(new GestureFrame(currentTime, currentOrientation, currentPotCurls));
        
        if (recordingStatusText != null)
        {
            recordingStatusText.text = $"Recording: {_currentGestureData.gestureName} ({currentTime:F2}s)";
        }
    }

    // --- Public methods to be called by UI Buttons ---
    public void StartRecordingGesture()
    {
        if (_isRecording)
        {
            Debug.LogWarning("GestureRecordingManager: Already recording a gesture.");
            if (recordingStatusText != null) recordingStatusText.text = "Already recording!";
            return;
        }

        string gestureName = "UnnamedGesture"; // Default if input field is null
        if (gestureNameInputField != null) {
            if (!string.IsNullOrWhiteSpace(gestureNameInputField.text))
            {
                gestureName = gestureNameInputField.text;
            }
            else
            {
                Debug.LogError("GestureRecordingManager: Gesture name cannot be empty to start recording.");
                if (recordingStatusText != null) recordingStatusText.text = "Error: Enter gesture name!";
                return;
            }
        } else {
             Debug.LogWarning("GestureRecordingManager: GestureNameInputField not assigned, using default name.");
        }
        

        _currentGestureData = new GestureData(gestureName);
        _recordingStartTime = Time.time;
        _isRecording = true;
        Debug.Log($"GestureRecordingManager: Started recording gesture: {gestureName}");
        if (recordingStatusText != null) recordingStatusText.text = $"Recording: {gestureName}...";
    }

    public void StopRecordingAndSaveGesture()
    {
        if (!_isRecording || _currentGestureData == null)
        {
            Debug.LogWarning("GestureRecordingManager: Not currently recording or no gesture data.");
            if (recordingStatusText != null) recordingStatusText.text = "Not recording.";
            return;
        }

        _isRecording = false;
        float rawDuration = 0f;
        if (_currentGestureData.frames.Count > 0)
        {
            // Timestamp of the last frame is its duration relative to the recording start
            rawDuration = _currentGestureData.frames[_currentGestureData.frames.Count - 1].timestamp;
        }

        Debug.Log($"GestureRecordingManager: Stopped recording '{_currentGestureData.gestureName}'. Raw duration: {rawDuration:F2}s. Frames captured: {_currentGestureData.frames.Count}");
        if (recordingStatusText != null) recordingStatusText.text = "Processing recorded gesture...";

        // --- Trimming Logic ---
        List<GestureFrame> trimmedFrames = new List<GestureFrame>();
        if (_currentGestureData.frames.Count > 0)
        {
            float effectiveEndTimeForTrimming = rawDuration - trimDurationEndSec;
            // Ensure effective end time is not before effective start time
            if (effectiveEndTimeForTrimming < trimDurationStartSec)
            {
                effectiveEndTimeForTrimming = trimDurationStartSec; 
                Debug.LogWarning("GestureRecordingManager: Trim duration is too long for the recorded gesture. Resulting gesture might be very short or empty.");
            }
            
            foreach (GestureFrame frame in _currentGestureData.frames)
            {
                if (frame.timestamp >= trimDurationStartSec && frame.timestamp <= effectiveEndTimeForTrimming)
                {
                    // Adjust timestamp to be relative to the start of the trimmed data segment
                    trimmedFrames.Add(new GestureFrame(
                        frame.timestamp - trimDurationStartSec,
                        frame.handOrientation,
                        frame.fingerCurls
                    ));
                }
            }
            _currentGestureData.frames = trimmedFrames; // Replace original frames with trimmed ones

            if (trimmedFrames.Count > 0)
            {
                _currentGestureData.totalDuration = trimmedFrames[trimmedFrames.Count - 1].timestamp;
            }
            else
            {
                _currentGestureData.totalDuration = 0f;
            }
            Debug.Log($"GestureRecordingManager: Frames after trimming: {_currentGestureData.frames.Count}. New gesture duration: {_currentGestureData.totalDuration:F2}s");
        }
        else
        {
             _currentGestureData.totalDuration = 0f; // No frames to begin with
        }
        
        if (_currentGestureData.frames.Count == 0)
        {
            Debug.LogWarning("GestureRecordingManager: No frames captured after trimming (or to begin with). Gesture not saved.");
            if (recordingStatusText != null) recordingStatusText.text = "No valid data recorded to save.";
            _currentGestureData = null; // Discard
            return;
        }

        // --- Use GestureStorageManager to save ---
        if (gestureStorageManager.SaveGestureData(_currentGestureData))
        {
            if (recordingStatusText != null) recordingStatusText.text = $"Saved: {_currentGestureData.gestureName}!";
            if (gestureNameInputField != null) gestureNameInputField.text = ""; // Clear input field
            // GestureStorageManager.SaveGestureData now calls LoadAllGestures, which fires an event
            // HandController (or future GestureSimulationManager) will listen to that event to update its dropdown.
        }
        else
        {
            if (recordingStatusText != null) recordingStatusText.text = "Error saving gesture!";
        }
        _currentGestureData = null; // Clear for next recording
    }
}