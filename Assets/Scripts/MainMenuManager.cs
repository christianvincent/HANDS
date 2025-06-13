// MainMenuManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO.Ports;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown rightHandDropdown;
    public TMP_Dropdown leftHandDropdown;
    public Button startButton;
    public Button exitButton;

    void Start()
    {
        // Get all available COM ports on the computer
        string[] availablePorts = SerialPort.GetPortNames();
        
        // Create a list from the array for easier manipulation
        List<string> portOptions = new List<string>(availablePorts);

        // Clear any existing options in the dropdowns
        rightHandDropdown.ClearOptions();
        leftHandDropdown.ClearOptions();
        
        if (portOptions.Count == 0)
        {
            portOptions.Add("No Ports Found");
            startButton.interactable = false; // Disable start button if no ports
        }
        
        // Add the found port names to both dropdowns
        rightHandDropdown.AddOptions(portOptions);
        leftHandDropdown.AddOptions(portOptions);

        // Add listeners to the buttons
        startButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    public void StartGame()
    {
        // Save the selected port names to our static class
        ConnectionSettings.RightHandPort = rightHandDropdown.options[rightHandDropdown.value].text;
        ConnectionSettings.LeftHandPort = leftHandDropdown.options[leftHandDropdown.value].text;

        Debug.Log($"Starting game with Right Port: {ConnectionSettings.RightHandPort} and Left Port: {ConnectionSettings.LeftHandPort}");

        // Load the main scene
        SceneManager.LoadScene("SampleScene"); // Make sure your main scene is named "SampleScene"
    }

    public void ExitGame()
    {
        Debug.Log("Exiting application.");
        Application.Quit();
    }
}