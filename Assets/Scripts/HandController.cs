// HandController.cs (Now primarily a coordinator/dependency holder)
using UnityEngine;
using TMPro; // Required if any TMP_ UIs were to remain, but they are moving out

// Note: All data structures (GestureFrame, GestureData, Finger etc.)
// should be defined in your DataTypes.cs file.

public class HandController : MonoBehaviour
{
    [Header("Core System Component Dependencies")]
    public InputDataManager inputDataManager; 
    public HandAnimator handAnimator;         
    public GestureStorageManager gestureStorageManager; 
    public GestureRecordingManager gestureRecordingManager; 
    public GestureSimulationManager gestureSimulationManager; // --- NEW: Assign GestureSimulationManager ---

    // All UI element references (InputFields, Texts, Dropdowns) have been moved 
    // to GestureRecordingManager.cs and GestureSimulationManager.cs.

    // All state variables (loadedGestures, _isRecording, etc.) have been moved.
    // All methods for recording, saving, loading, simulating gestures have been moved.

    void Start()
    {
        // Perform essential checks to ensure all managers are assigned.
        // Without these, the system cannot function.
        if (inputDataManager == null) {
            Debug.LogError("HandController FATAL ERROR: InputDataManager not assigned!");
            enabled = false; return; // Critical failure
        }
        if (handAnimator == null) {
            Debug.LogError("HandController FATAL ERROR: HandAnimator not assigned!");
            enabled = false; return; // Critical failure
        }
        if (gestureStorageManager == null) {
            Debug.LogError("HandController FATAL ERROR: GestureStorageManager not assigned!");
            enabled = false; return; // Critical failure
        }
        if (gestureRecordingManager == null) {
            Debug.LogError("HandController FATAL ERROR: GestureRecordingManager not assigned!");
            enabled = false; return; // Critical failure
        }
        if (gestureSimulationManager == null) {
            Debug.LogError("HandController FATAL ERROR: GestureSimulationManager not assigned!");
            enabled = false; return; // Critical failure
        }

        Debug.Log("HandController: All core system components assigned and validated. System starting.");
        // Any high-level initial coordination can happen here if needed,
        // but most managers now initialize themselves and communicate via events or direct references.
    }

    void Update() 
    {
        // The HandController's Update() is now very minimal or empty.
        // Each manager (InputDataManager, HandAnimator, GestureRecordingManager)
        // handles its own Update logic if necessary.
        // For example, HandAnimator updates the 3D model.
        // GestureRecordingManager updates its recording status text and captures frames.
    }

    void OnApplicationQuit() 
    {
        // Managers should handle their own cleanup (e.g., SerialConnectionManager).
        Debug.Log("HandController: Application quitting.");
    }

    // All other public methods previously here for UI button calls
    // (StartRecordingGesture, StopRecordingAndSaveGesture, SimulateSelectedGestureFromDropdown,
    // ReloadGesturesButtonHandler, DisplayRecognizedGesture)
    // have been moved to either GestureRecordingManager.cs or GestureSimulationManager.cs.
    // Your UI Buttons need to be re-linked to call these methods on instances of those new manager scripts.
}