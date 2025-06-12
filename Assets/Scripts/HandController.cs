// HandController.cs
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Core System Component Dependencies")]
    [Tooltip("Manages receiving and parsing data for the RIGHT hand.")]
    public InputDataManager rightInputManager;

    [Tooltip("Manages receiving and parsing data for the LEFT hand.")]
    public InputDataManager leftInputManager;

    [Tooltip("Manages animating the RIGHT 3D hand model.")]
    public HandAnimator rightHandAnimator;

    [Tooltip("Manages animating the LEFT 3D hand model.")]
    public HandAnimator leftHandAnimator;

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
        bool criticalDependencyMissing = false;

        if (rightInputManager == null) { Debug.LogError("HandController FATAL ERROR: Right InputDataManager not assigned!"); criticalDependencyMissing = true; }
        if (leftInputManager == null) { Debug.LogError("HandController FATAL ERROR: Left InputDataManager not assigned!"); criticalDependencyMissing = true; }
        if (rightHandAnimator == null) { Debug.LogError("HandController FATAL ERROR: Right HandAnimator not assigned!"); criticalDependencyMissing = true; }
        if (leftHandAnimator == null) { Debug.LogError("HandController FATAL ERROR: Left HandAnimator not assigned!"); criticalDependencyMissing = true; }
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