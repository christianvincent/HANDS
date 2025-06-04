// GestureSimulationManager.cs
using UnityEngine;
using TMPro; // For TextMeshPro elements
using System.Collections.Generic; // For List
using System;

// Note: GestureData struct/class should be defined in your DataTypes.cs file

public class GestureSimulationManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GestureStorageManager gestureStorageManager; // Assign your GestureStorageManager

    [Header("Simulation UI Elements")]
    public TMP_Text recognizedGestureText;        // Assign UI Text for output
    public TMP_Dropdown gestureSelectionDropdown;   // Assign UI Dropdown for selection

    void Start()
    {
        if (gestureStorageManager == null)
        {
            Debug.LogError("GestureSimulationManager: GestureStorageManager not assigned! Simulation UI will not function correctly.");
            enabled = false; return;
        }
        if (recognizedGestureText == null) Debug.LogWarning("GestureSimulationManager: Recognized Gesture Text not assigned.");
        if (gestureSelectionDropdown == null) Debug.LogWarning("GestureSimulationManager: Gesture Selection Dropdown not assigned.");

        // Subscribe to the event from GestureStorageManager to update the dropdown when gestures are loaded/reloaded
        gestureStorageManager.OnGesturesReloaded += PopulateGestureDropdown;
        
        // Initial population of the dropdown
        PopulateGestureDropdown();
    }

    void OnDestroy()
    {
        // Unsubscribe from the event when this object is destroyed
        if (gestureStorageManager != null)
        {
            gestureStorageManager.OnGesturesReloaded -= PopulateGestureDropdown;
        }
    }

    private void PopulateGestureDropdown()
    {
        if (gestureSelectionDropdown == null || gestureStorageManager == null) return;

        gestureSelectionDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();

        if (gestureStorageManager.AllLoadedGestures.Count == 0)
        {
            dropdownOptions.Add(new TMP_Dropdown.OptionData("No gestures saved"));
            if (recognizedGestureText != null) recognizedGestureText.text = "No gestures available to simulate.";
        }
        else
        {
            foreach (GestureData gesture in gestureStorageManager.AllLoadedGestures)
            {
                dropdownOptions.Add(new TMP_Dropdown.OptionData(gesture.gestureName));
            }
            if (recognizedGestureText != null) recognizedGestureText.text = $"Select a gesture to simulate ({gestureStorageManager.AllLoadedGestures.Count} loaded).";
        }
        
        gestureSelectionDropdown.AddOptions(dropdownOptions);
        if (dropdownOptions.Count > 0 && !dropdownOptions[0].text.StartsWith("No gestures"))
        {
            gestureSelectionDropdown.value = 0; // Select the first valid gesture by default
        }
        gestureSelectionDropdown.RefreshShownValue(); // Important to update the displayed value
    }
    
    // --- Public methods to be called by UI Buttons ---
    public void SimulateSelectedGestureFromDropdown()
    {
        if (gestureSelectionDropdown == null || gestureStorageManager.AllLoadedGestures.Count == 0 || 
            gestureSelectionDropdown.options.Count == 0 || gestureSelectionDropdown.value < 0 || 
            gestureSelectionDropdown.value >= gestureSelectionDropdown.options.Count ||
            gestureSelectionDropdown.options[gestureSelectionDropdown.value].text.StartsWith("No gestures")) 
        {
            if (recognizedGestureText != null) recognizedGestureText.text = "No valid gesture selected to simulate."; 
            Debug.LogWarning("GestureSimulationManager: No valid gesture selected in dropdown for simulation.");
            return;
        }

        string selectedGestureName = gestureSelectionDropdown.options[gestureSelectionDropdown.value].text;
        DisplayRecognizedGesture(selectedGestureName);
    }

    // This method updates the UI Text with the "recognized" gesture name.
    public void DisplayRecognizedGesture(string gestureName) 
    {
        if (string.IsNullOrWhiteSpace(gestureName) || gestureName.ToLower().Contains("no gestures")) 
        {
            if (recognizedGestureText != null) recognizedGestureText.text = "Please select a valid gesture.";
            return;
        }
        
        // Optionally, verify against GestureStorageManager again, though dropdown should be accurate
        bool found = gestureStorageManager.AllLoadedGestures.Exists(g => g.gestureName == gestureName);

        if (recognizedGestureText != null) 
        {
            if (found) 
            {
                recognizedGestureText.text = $"Gesture (Simulated): {gestureName}";
                Debug.Log($"GestureSimulationManager: Simulated recognition and displayed: {gestureName}");
            }
            else 
            {
                // This state should ideally not be reached if dropdown is synced
                recognizedGestureText.text = $"Error: Gesture '{gestureName}' not found in storage for simulation.";
                Debug.LogWarning($"GestureSimulationManager: Attempted to display '{gestureName}', but it's not in GestureStorageManager's loaded list.");
            }
        }
    }

    // This button handler now specifically tells GestureStorageManager to reload,
    // which will then trigger the OnGesturesReloaded event, and PopulateGestureDropdown will run.
    public void ReloadGesturesButtonHandler() 
    { 
        if (gestureStorageManager != null)
        {
            Debug.Log("GestureSimulationManager: Requesting gesture reload from storage manager...");
            gestureStorageManager.LoadAllGestures(); 
        }
        else
        {
            Debug.LogError("GestureSimulationManager: GestureStorageManager not assigned. Cannot reload gestures.");
        }
    }
}