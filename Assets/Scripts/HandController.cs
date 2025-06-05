// HandController.cs (Final Refactored Version - Coordinator)
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Core System Component Dependencies")]
    [Tooltip("Manages receiving and parsing data from the ESP32.")]
    public InputDataManager inputDataManager; 
    
    [Tooltip("Manages animating the 3D hand model.")]
    public HandAnimator handAnimator;         
    
    [Tooltip("Manages loading and saving gesture data to files.")]
    public GestureStorageManager gestureStorageManager; 
    
    [Tooltip("Manages the UI and logic for recording new gestures.")]
    public GestureRecordingManager gestureRecordingManager; 
    
    [Tooltip("Manages the UI and logic for simulating gesture recognition.")]
    public GestureSimulationManager gestureSimulationManager; 
    
    [Tooltip("Manages the UI and logic for calibrating hand poses.")]
    public HandCalibrationManager handCalibrationManager; 

    void Start()
    {
        // This script's primary role is to ensure all systems are in place.
        // It validates that all necessary manager components have been assigned in the Inspector.
        bool criticalDependencyMissing = false;
        if (inputDataManager == null) { Debug.LogError("HandController FATAL ERROR: InputDataManager not assigned!"); criticalDependencyMissing = true; }
        if (handAnimator == null) { Debug.LogError("HandController FATAL ERROR: HandAnimator not assigned!"); criticalDependencyMissing = true; }
        if (gestureStorageManager == null) { Debug.LogError("HandController FATAL ERROR: GestureStorageManager not assigned!"); criticalDependencyMissing = true; }
        if (gestureRecordingManager == null) { Debug.LogError("HandController FATAL ERROR: GestureRecordingManager not assigned!"); criticalDependencyMissing = true; }
        if (gestureSimulationManager == null) { Debug.LogError("HandController FATAL ERROR: GestureSimulationManager not assigned!"); criticalDependencyMissing = true; }
        if (handCalibrationManager == null) { Debug.LogError("HandController FATAL ERROR: HandCalibrationManager not assigned!"); criticalDependencyMissing = true; }

        if (criticalDependencyMissing)
        {
            Debug.LogError("HandController disabled due to missing critical dependencies. Check Inspector assignments on the HandController GameObject.");
            enabled = false; 
            return;
        }

        Debug.Log("HandController: All core system components assigned and validated. System is operational.");
    }
}