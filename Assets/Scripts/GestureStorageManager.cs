// GestureStorageManager.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

// Note: GestureData struct/class should be defined in your DataTypes.cs file

public class GestureStorageManager : MonoBehaviour
{
    [Header("Configuration")]
    public string gestureSaveSubfolder = "GestureData"; // Subfolder within Application.persistentDataPath

    // Public property to access all loaded gestures
    public List<GestureData> AllLoadedGestures { get; private set; } = new List<GestureData>();

    public event Action OnGesturesReloaded; // Event to notify when gestures have been reloaded

    private string GetFullSaveDirectoryPath()
    {
        return Path.Combine(Application.persistentDataPath, gestureSaveSubfolder);
    }

    void Awake()
    {
        // Ensure the save directory exists when the game starts
        try
        {
            string fullPath = GetFullSaveDirectoryPath();
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"GestureStorageManager: Created gesture save directory at: {fullPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GestureStorageManager: Error creating gesture save directory: {e.Message}");
        }
        
        LoadAllGestures(); // Load any existing gestures at the start
    }

    public void LoadAllGestures()
    {
        AllLoadedGestures.Clear();
        string directoryPath = GetFullSaveDirectoryPath();

        if (!Directory.Exists(directoryPath))
        {
            Debug.LogWarning($"GestureStorageManager: Gesture save directory not found: {directoryPath}. No gestures loaded.");
            OnGesturesReloaded?.Invoke(); // Notify even if none found, so UI can update
            return;
        }

        string[] gestureFiles = Directory.GetFiles(directoryPath, "*.json");
        Debug.Log($"GestureStorageManager: Found {gestureFiles.Length} gesture files in {directoryPath}.");

        foreach (string filePath in gestureFiles)
        {
            try
            {
                string jsonInput = File.ReadAllText(filePath);
                GestureData gesture = JsonUtility.FromJson<GestureData>(jsonInput);
                if (gesture != null && gesture.frames != null && !string.IsNullOrEmpty(gesture.gestureName))
                {
                    AllLoadedGestures.Add(gesture);
                    Debug.Log($"GestureStorageManager: Loaded gesture: {gesture.gestureName} ({gesture.frames.Count} frames).");
                }
                else
                {
                    Debug.LogWarning($"GestureStorageManager: Failed to properly parse gesture data or name missing from: {filePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"GestureStorageManager: Error loading gesture file {filePath}: {e.Message}");
            }
        }
        Debug.Log($"GestureStorageManager: Total gestures loaded: {AllLoadedGestures.Count}");
        OnGesturesReloaded?.Invoke(); // Notify listeners that gestures have been reloaded
    }

    public bool SaveGestureData(GestureData gestureToSave)
    {
        if (gestureToSave == null || string.IsNullOrWhiteSpace(gestureToSave.gestureName) || gestureToSave.frames == null)
        {
            Debug.LogError("GestureStorageManager: Attempted to save null or invalid gesture data.");
            return false;
        }

        try
        {
            string sanitizedGestureName = string.Join("_", gestureToSave.gestureName.Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(sanitizedGestureName) || sanitizedGestureName.Length > 60)
            {
                // Fallback to a timestamped name if sanitized name is problematic or too long
                sanitizedGestureName = "gesture_" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
            }

            string directoryPath = GetFullSaveDirectoryPath();
            // Ensure directory exists (it should from Awake, but good to be safe)
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, sanitizedGestureName + ".json");
            string jsonOutput = JsonUtility.ToJson(gestureToSave, true); // true for pretty print
            File.WriteAllText(filePath, jsonOutput);

            Debug.Log($"GestureStorageManager: Gesture '{gestureToSave.gestureName}' saved to: {filePath}");
            
            // After saving, reload all gestures so the internal list and any UI is up-to-date
            LoadAllGestures(); 
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"GestureStorageManager: Error saving gesture '{gestureToSave.gestureName}': {e.Message}");
            return false;
        }
    }
}