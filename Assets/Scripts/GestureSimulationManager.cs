// GestureSimulationManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GestureSimulationManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GestureStorageManager gestureStorageManager;

    [Header("Simulation UI Elements")]
    public TMP_Text recognizedGestureText;
    public TMP_Dropdown gestureSelectionDropdown;
    public Button simulateButton;
    public Button reloadButton;

    void Start()
    {
        if (gestureStorageManager == null)
        {
            Debug.LogError("GestureSimulationManager: GestureStorageManager not assigned!", this.gameObject);
            enabled = false; return;
        }

        // Subscribe to the event to automatically update the dropdown when gestures are loaded/saved.
        gestureStorageManager.OnGesturesReloaded += PopulateGestureDropdown;
        
        // Initial population
        PopulateGestureDropdown();

        if (simulateButton) simulateButton.onClick.AddListener(SimulateSelectedGestureFromDropdown);
        if (reloadButton) reloadButton.onClick.AddListener(ReloadGesturesButtonHandler);
    }

    void OnDestroy()
    {
        if (gestureStorageManager != null)
        {
            gestureStorageManager.OnGesturesReloaded -= PopulateGestureDropdown;
        }
    }

    private void PopulateGestureDropdown()
    {
        if (gestureSelectionDropdown == null) return;

        gestureSelectionDropdown.ClearOptions();
        List<string> gestureNames = new List<string>();

        if (gestureStorageManager.AllLoadedGestures.Count == 0)
        {
            gestureNames.Add("No gestures saved");
        }
        else
        {
            foreach (GestureData gesture in gestureStorageManager.AllLoadedGestures)
            {
                gestureNames.Add(gesture.gestureName);
            }
        }
        
        gestureSelectionDropdown.AddOptions(gestureNames);
        UpdateStatusText();
    }
    
    public void SimulateSelectedGestureFromDropdown()
    {
        if (gestureStorageManager.AllLoadedGestures.Count == 0 || gestureSelectionDropdown.value < 0)
        {
            if (recognizedGestureText) recognizedGestureText.text = "No valid gesture selected.";
            return;
        }

        string selectedGestureName = gestureSelectionDropdown.options[gestureSelectionDropdown.value].text;
        if (recognizedGestureText != null)
        {
            recognizedGestureText.text = $"Gesture (Simulated): {selectedGestureName}";
            Debug.Log($"Simulated recognition and displayed: {selectedGestureName}");
        }
    }

    private void ReloadGesturesButtonHandler()
    {
        gestureStorageManager.LoadAllGestures();
    }

    private void UpdateStatusText()
    {
        if (recognizedGestureText != null)
        {
             recognizedGestureText.text = $"Select a gesture to simulate ({gestureStorageManager.AllLoadedGestures.Count} loaded).";
        }
    }
}