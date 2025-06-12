// HandCalibrationManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HandCalibrationManager : MonoBehaviour
{
    // All variable declarations are the same
    [Header("Hand System Dependencies")]
    public InputDataManager rightInputManager;
    public InputDataManager leftInputManager;
    public HandAnimator rightHandAnimator;
    public HandAnimator leftHandAnimator;

    [Header("UI Elements")]
    public GameObject calibrationPanel;
    public TMP_Text instructionText;
    public Button nextStepButton;
    public Button finishCalibrationButton;
    public Button cancelCalibrationButton;
    public Button calibrateRightHandButton;
    public Button calibrateLeftHandButton;

    private const int NUM_FINGERS = 5;
    private enum CalibrationState { Idle, CalibratingFingerOpen, CalibratingFingerClosed, CalibratingNeutralPose, Done }
    private CalibrationState _currentState = CalibrationState.Idle;
    private int _currentFingerIndexToCalibrate = 0;
    
    private bool _isButtonActionable = true;

    private InputDataManager _activeInputManager;
    private HandAnimator _activeHandAnimator;
    private HandType _activeHandType;

    private int[] _tempMinPotValues = new int[NUM_FINGERS];
    private int[] _tempMaxPotValues = new int[NUM_FINGERS];
    private Quaternion _tempNeutralOrientation;
    private readonly string[] fingerNames = { "Thumb", "Index", "Middle", "Ring", "Pinky" };

    // The Start() method is unchanged from the last version
    void Start()
    {
        if (rightInputManager == null || leftInputManager == null || rightHandAnimator == null || leftHandAnimator == null) { Debug.LogError("HandCalibrationManager: Hand system dependencies not assigned!"); enabled = false; return; }
        if (calibrationPanel == null || instructionText == null || nextStepButton == null || finishCalibrationButton == null || cancelCalibrationButton == null || calibrateRightHandButton == null || calibrateLeftHandButton == null) { Debug.LogError("HandCalibrationManager: UI elements not assigned!"); enabled = false; return; }
        
        calibrateRightHandButton.onClick.RemoveAllListeners();
        calibrateRightHandButton.onClick.AddListener(() => SelectHandForCalibration(HandType.Right));
        calibrateLeftHandButton.onClick.RemoveAllListeners();
        calibrateLeftHandButton.onClick.AddListener(() => SelectHandForCalibration(HandType.Left));
        nextStepButton.onClick.RemoveAllListeners();
        nextStepButton.onClick.AddListener(OnNextStepButtonPressed);
        finishCalibrationButton.onClick.RemoveAllListeners();
        finishCalibrationButton.onClick.AddListener(ApplyAndSaveChanges);
        cancelCalibrationButton.onClick.RemoveAllListeners();
        cancelCalibrationButton.onClick.AddListener(CancelCalibration);
        
        calibrationPanel.SetActive(false);
    }
    
    // SelectHandForCalibration and StartFullCalibrationProcess are the same
    public void SelectHandForCalibration(HandType hand)
    {
        _activeHandType = hand;
        _activeInputManager = (hand == HandType.Right) ? rightInputManager : leftInputManager;
        _activeHandAnimator = (hand == HandType.Right) ? rightHandAnimator : leftHandAnimator;
        Debug.Log($"Selected {_activeHandType} hand for calibration.");
        StartFullCalibrationProcess();
    }

    private void StartFullCalibrationProcess()
    {
        rightHandAnimator.IsAnimationPaused = true;
        leftHandAnimator.IsAnimationPaused = true;
        calibrationPanel.SetActive(true);
        calibrateRightHandButton.gameObject.SetActive(false);
        calibrateLeftHandButton.gameObject.SetActive(false);
        finishCalibrationButton.gameObject.SetActive(false);
        nextStepButton.gameObject.SetActive(true);
        _currentFingerIndexToCalibrate = 0;
        _currentState = CalibrationState.CalibratingFingerOpen;
        _isButtonActionable = true;
        UpdateInstructionText();
    }

    public void OnNextStepButtonPressed()
    {
        if (!_isButtonActionable) return;
        if (_activeInputManager == null || _activeInputManager.RawPotValues == null)
        {
            Debug.LogError("Calibration error: Active Input Manager is not set or not receiving data.");
            return;
        }

        _isButtonActionable = false;

        if (_currentState == CalibrationState.CalibratingFingerOpen)
        {
            _tempMinPotValues[_currentFingerIndexToCalibrate] = _activeInputManager.RawPotValues[_currentFingerIndexToCalibrate];
            Debug.Log($"Captured OPEN for {_activeHandType} {fingerNames[_currentFingerIndexToCalibrate]}: {_tempMinPotValues[_currentFingerIndexToCalibrate]}");
            _currentState = CalibrationState.CalibratingFingerClosed;
        }
        else if (_currentState == CalibrationState.CalibratingFingerClosed)
        {
            _tempMaxPotValues[_currentFingerIndexToCalibrate] = _activeInputManager.RawPotValues[_currentFingerIndexToCalibrate];
            Debug.Log($"Captured CLOSED for {_activeHandType} {fingerNames[_currentFingerIndexToCalibrate]}: {_tempMaxPotValues[_currentFingerIndexToCalibrate]}");

            // --- THIS IS THE FIX ---
            // The block of code that swapped the min/max values has been REMOVED.
            // This allows the system to correctly handle both normal and inverted potentiometers.
            
            _currentFingerIndexToCalibrate++;

            if (_currentFingerIndexToCalibrate < NUM_FINGERS)
            {
                _currentState = CalibrationState.CalibratingFingerOpen;
            }
            else
            {
                _currentState = CalibrationState.CalibratingNeutralPose;
            }
        }
        else if (_currentState == CalibrationState.CalibratingNeutralPose)
        {
            _tempNeutralOrientation = _activeInputManager.TargetHandOrientation;
            Debug.Log($"Captured NEUTRAL orientation for {_activeHandType}: {_tempNeutralOrientation.eulerAngles}");
            _currentState = CalibrationState.Done;
        }

        UpdateInstructionText();
        StartCoroutine(ButtonCooldown());
    }
    
    // The rest of the script is unchanged
    private IEnumerator ButtonCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        _isButtonActionable = true;
    }

    private void UpdateInstructionText()
    {
        if (instructionText == null) return;
        string handSide = _activeHandType.ToString();
        TMP_Text nextButtonText = nextStepButton.GetComponentInChildren<TMP_Text>();

        switch (_currentState)
        {
            case CalibrationState.CalibratingFingerOpen:
                instructionText.text = $"({handSide} Hand) Hold '{fingerNames[_currentFingerIndexToCalibrate]}' finger FULLY OPEN.\nPress 'Capture Open Pose'.";
                if (nextButtonText != null) nextButtonText.text = "Capture Open Pose";
                break;
            case CalibrationState.CalibratingFingerClosed:
                instructionText.text = $"({handSide} Hand) Make a tight FIST, curling '{fingerNames[_currentFingerIndexToCalibrate]}'.\nPress 'Capture Closed Pose'.";
                if (nextButtonText != null) nextButtonText.text = "Capture Closed Pose";
                break;
            case CalibrationState.CalibratingNeutralPose:
                instructionText.text = $"({handSide} Hand) Hold hand FLAT, PALM-DOWN.\nPress 'Set Neutral Orientation'.";
                if (nextButtonText != null) nextButtonText.text = "Set Neutral Orientation";
                break;
            case CalibrationState.Done:
                instructionText.text = "Calibration data captured! Press 'Apply & Save' or 'Cancel'.";
                nextStepButton.gameObject.SetActive(false);
                finishCalibrationButton.gameObject.SetActive(true);
                break;
        }
    }
    
    public void ApplyAndSaveChanges()
    {
        if (_activeInputManager == null) return;
        Debug.Log($"Applying and saving calibration data for {_activeHandType} hand...");
        for (int i = 0; i < NUM_FINGERS; i++)
        {
            // Note: We check if Min and Max are different to avoid a "divide by zero" issue in InverseLerp
            if (_tempMaxPotValues[i] != _tempMinPotValues[i])
            {
                _activeInputManager.SetPotentiometerCalibrationData(i, _tempMinPotValues[i], _tempMaxPotValues[i]);
            }
            else
            {
                Debug.LogWarning($"Skipping calibration for finger {i} because min and max values are the same.");
            }
        }
        if (_currentState == CalibrationState.Done)
        {
            _activeInputManager.SetNeutralOrientation(_tempNeutralOrientation);
        }
        _activeInputManager.SaveCalibrationSettings();
        EndCalibrationSession("Calibration Applied and Saved!");
    }

    public void CancelCalibration()
    {
        EndCalibrationSession("Calibration Cancelled.");
    }

    private void EndCalibrationSession(string message)
    {
        instructionText.text = message;
        calibrationPanel.SetActive(false);
        calibrateRightHandButton.gameObject.SetActive(true);
        calibrateLeftHandButton.gameObject.SetActive(true);
        rightHandAnimator.IsAnimationPaused = false;
        leftHandAnimator.IsAnimationPaused = false;
        _currentState = CalibrationState.Idle;
        _activeInputManager = null;
    }
}