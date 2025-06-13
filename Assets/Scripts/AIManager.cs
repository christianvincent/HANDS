// AIManager.cs
using UnityEngine;
using TMPro; // We need this for the UI elements
using UnityEngine.UI;
using Glitch9; // This is the namespace from the new asset

public class AIManager : MonoBehaviour
{
    [Header("UI For Testing")]
    [Tooltip("An input field to type the text you want to speak.")]
    public TMP_InputField textToSpeakInput;
    [Tooltip("A button to trigger the speech.")]
    public Button speakButton;

    void Start()
    {
        // Set up the listener for our test button
        if (speakButton != null)
        {
            speakButton.onClick.AddListener(OnSpeakButtonClick);
        }
        else
        {
            Debug.LogWarning("Speak Button is not assigned in the AIManager.");
        }
    }

    /// <summary>
    /// This is the main public function that other scripts will call to make the AI speak.
    /// </summary>
    /// <param name="textToSpeak">The sentence you want to be spoken.</param>
    public void Speak(string textToSpeak)
    {
        if (string.IsNullOrWhiteSpace(textToSpeak))
        {
            Debug.LogWarning("Speak command was called with empty text.");
            return;
        }

        Debug.Log($"Sending '{textToSpeak}' to the Text-to-Speech service...");

        // This is the magic line from the AI Dev Kit.
        // It handles all the complex work of contacting the server and playing the audio.
        // We can change "en-US" to "tl-PH" later to test Tagalog.
        AI.TextToSpeech(textToSpeak, "en-US");
    }

    // This private method is just for our UI test button.
    private void OnSpeakButtonClick()
    {
        // When the button is clicked, it calls the main Speak function
        // with whatever text is in the input field.
        if (textToSpeakInput != null)
        {
            Speak(textToSpeakInput.text);
        }
    }
}