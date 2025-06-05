// GestureSimulationManager.cs (Corrected Final Version)
using UnityEngine;
using UnityEngine.UI;   // --- ADDED THIS LINE to recognize the 'Button' type ---
using TMPro;          // For TextMeshPro elements
using System.Collections.Generic;
using System;

// Note: GestureData struct/class should be defined in your DataTypes.cs file

public class GestureSimulationManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GestureStorageManager gestureStorageManager; // Assign your GestureStorageManager

    [Header("Simulation UI Elements")]
    public TMP_Text recognizedGestureText;        // Assign UI Text for output
    public TMP_Dropdown gestureSelectionDropdown;   // Assign UI Dropdown for selection
    public Button simulateButton;                 // Assign your "Simulate Selected Gesture" button
    public Button reloadButton;                   // Optional: Assign a "Reload Gestures" button

    void Start()
    {
        // --- Dependency Checks ---
        if (gestureStorageManager == null)
        {
            Debug.LogError("GestureSimulationManager: GestureStorageManager not assigned! UI will not function.");
            enabled = false; return;
        }
        if (recognizedGestureText == null) Debug.LogWarning("GestureSimulationManager: Recognized Gesture Text not assigned.");
        if (gestureSelectionDropdown == null) Debug.LogWarning("GestureSimulationManager: Gesture Selection Dropdown not assigned.");
        if (simulateButton == null) Debug.LogWarning("GestureSimulationManager: Simulate Button not assigned.");

        // --- Add Listeners for UI Buttons ---
        if (simulateButton != null)
        {
            simulateButton.onClick.AddListener(SimulateSelectedGestureFromDropdown);
        }
        if (reloadButton != null)
        {
            reloadButton.onClick.AddListener(ReloadGesturesButtonHandler);
        }

        // --- Subscribe to Event from GestureStorageManager ---
        // This makes sure our dropdown updates whenever gestures are loaded/reloaded.
        gestureStorageManager.OnGesturesReloaded += PopulateGestureDropdown;
        
        // --- Initial Population of the Dropdown ---
        PopulateGestureDropdown();
    }

    void OnDestroy()
    {
        // --- Unsubscribe from Event ---
        // This is important to prevent errors when objects are destroyed.
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
            if (recognizedGestureText != null) recognizedGestureText.text = "No gestures available.";
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
            gestureSelectionDropdown.value = 0;
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
            if (recognizedGestureText != null) recognizedGestureText.text = "No valid gesture selected."; 
            Debug.LogWarning("GestureSimulationManager: No valid gesture selected in dropdown for simulation.");
            return;
        }

        string selectedGestureName = gestureSelectionDropdown.options[gestureSelectionDropdown.value].text;
        DisplayRecognizedGesture(selectedGestureName);
    }

    private void DisplayRecognizedGesture(string gestureName) 
    {
        if (string.IsNullOrWhiteSpace(gestureName) || gestureName.ToLower().Contains("no gestures")) 
        {
            if (recognizedGestureText != null) recognizedGestureText.text = "Please select a valid gesture.";
            return;
        }
        
        bool found = gestureStorageManager.AllLoadedGestures.Exists(g => g.gestureName == gestureName);

        if (recognizedGestureText != null) {
            if (found) 
            {
                recognizedGestureText.text = $"Gesture (Simulated): {gestureName}";
                Debug.Log($"GestureSimulationManager: Simulated recognition and displayed: {gestureName}");
            }
            else 
            {
                recognizedGestureText.text = $"Error: Gesture '{gestureName}' not in storage.";
                Debug.LogWarning($"GestureSimulationManager: Attempted to display '{gestureName}', but it's not in GestureStorageManager's loaded list.");
            }
        }
    }

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