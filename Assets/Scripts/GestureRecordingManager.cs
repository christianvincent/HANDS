// GestureRecordingManager.cs
using UnityEngine;
using TMPro;
using System;

public class GestureRecordingManager : MonoBehaviour
{
    [Header("Dependencies")]
    public InputDataManager rightInputManager;
    public InputDataManager leftInputManager;
    public GestureStorageManager gestureStorageManager;

    [Header("Recording UI Elements")]
    public TMP_InputField gestureNameInputField;
    public TMP_Text recordingStatusText;

    [Header("Recording Configuration")]
    public float trimDurationStartSec = 1.0f;
    public float trimDurationEndSec = 1.0f;

    // Recording state
    private bool _isRecording = false;
    private GestureData _currentGestureData;
    private float _recordingStartTime;

    void Start()
    {
        if (rightInputManager == null || leftInputManager == null)
        {
            Debug.LogError("GestureRecordingManager: One or both InputDataManagers not assigned!");
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
        if (!_isRecording || _currentGestureData == null)
        {
            return;
        }
        
        float currentTime = Time.time - _recordingStartTime;

        // Capture Right Hand Pose
        HandPose rightHandPose = new HandPose(
            // MODIFIED: Removed the parentheses from IsConnected
            rightInputManager.serialConnectionManager.IsConnected, // isTracked
            rightInputManager.TargetHandOrientation,
            rightInputManager.PotCurlTargets
        );

        // Capture Left Hand Pose
        HandPose leftHandPose = new HandPose(
            // MODIFIED: Removed the parentheses from IsConnected
            leftInputManager.serialConnectionManager.IsConnected, // isTracked
            leftInputManager.TargetHandOrientation,
            leftInputManager.PotCurlTargets
        );
        
        _currentGestureData.frames.Add(new GestureFrame(currentTime, rightHandPose, leftHandPose));
        
        if (recordingStatusText != null)
        {
            recordingStatusText.text = $"Recording: {_currentGestureData.gestureName} ({currentTime:F2}s)";
        }
    }

    public void StartRecordingGesture()
    {
        if (_isRecording)
        {
            Debug.LogWarning("GestureRecordingManager: Already recording a gesture.");
            return;
        }
        
        string gestureName = gestureNameInputField.text;
        if (string.IsNullOrWhiteSpace(gestureName))
        {
            Debug.LogError("GestureRecordingManager: Gesture name cannot be empty.");
            if (recordingStatusText != null) recordingStatusText.text = "Error: Enter gesture name!";
            return;
        }

        _currentGestureData = new GestureData(gestureName);
        _recordingStartTime = Time.time;
        _isRecording = true;
        Debug.Log($"Started recording gesture: {gestureName}");
        if (recordingStatusText != null) recordingStatusText.text = $"Recording: {gestureName}...";
    }

    public void StopRecordingAndSaveGesture()
    {
        if (!_isRecording || _currentGestureData == null)
        {
            Debug.LogWarning("GestureRecordingManager: Not currently recording.");
            return;
        }

        _isRecording = false;
        
        // The trimming logic works on the whole GestureFrame, so it doesn't need modification
        GestureData trimmedGesture = TrimGesture(_currentGestureData);

        if (trimmedGesture.frames.Count == 0)
        {
            Debug.LogWarning("No frames captured after trimming. Gesture not saved.");
            if (recordingStatusText != null) recordingStatusText.text = "No valid data to save.";
            _currentGestureData = null;
            return;
        }

        if (gestureStorageManager.SaveGestureData(trimmedGesture))
        {
            if (recordingStatusText != null) recordingStatusText.text = $"Saved: {trimmedGesture.gestureName}!";
            if (gestureNameInputField != null) gestureNameInputField.text = "";
        }
        else
        {
            if (recordingStatusText != null) recordingStatusText.text = "Error saving gesture!";
        }
        _currentGestureData = null;
    }

    private GestureData TrimGesture(GestureData originalGesture)
    {
        if (originalGesture.frames.Count == 0) return originalGesture;

        var trimmedFrames = new System.Collections.Generic.List<GestureFrame>();
        float rawDuration = originalGesture.frames[originalGesture.frames.Count - 1].timestamp;
        float effectiveEndTime = rawDuration - trimDurationEndSec;

        if (effectiveEndTime < trimDurationStartSec)
        {
            effectiveEndTime = trimDurationStartSec;
            Debug.LogWarning("Trim duration is too long for the recorded gesture.");
        }

        foreach (GestureFrame frame in originalGesture.frames)
        {
            if (frame.timestamp >= trimDurationStartSec && frame.timestamp <= effectiveEndTime)
            {
                trimmedFrames.Add(new GestureFrame(
                    frame.timestamp - trimDurationStartSec,
                    frame.rightHand,
                    frame.leftHand
                ));
            }
        }

        originalGesture.frames = trimmedFrames;
        originalGesture.totalDuration = (trimmedFrames.Count > 0)
            ? trimmedFrames[trimmedFrames.Count - 1].timestamp
            : 0f;

        Debug.Log($"Frames after trimming: {originalGesture.frames.Count}. New duration: {originalGesture.totalDuration:F2}s");
        return originalGesture;
    }
}