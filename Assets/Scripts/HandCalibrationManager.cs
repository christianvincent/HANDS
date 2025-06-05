// HandCalibrationManager.cs (Corrected)
using UnityEngine;
using UnityEngine.UI; 
using TMPro;        
using System.Collections.Generic; 

public class HandCalibrationManager : MonoBehaviour
{
    [Header("Dependencies")]
    public InputDataManager inputDataManager;
    public HandAnimator handAnimator; 

    [Header("UI Elements")]
    public TMP_Text instructionText;
    public Button nextStepButton;         
    public Button finishCalibrationButton; 
    public Button cancelCalibrationButton; 
    public Button startCalibrationButton; 

    private const int NUM_FINGERS = 5; 

    private enum CalibrationState { Idle, WaitingToStart, CalibratingFingerOpen, CalibratingFingerClosed, CalibratingNeutralPose, Done }
    private CalibrationState _currentState = CalibrationState.Idle;
    private int _currentFingerIndexToCalibrate = 0;

    private int[] _tempMinPotValues = new int[NUM_FINGERS];
    private int[] _tempMaxPotValues = new int[NUM_FINGERS];
    private Quaternion _tempNeutralOrientation;

    private string[] fingerNames = { "Thumb", "Index", "Middle", "Ring", "Pinky" }; 

    void Start()
    {
        if (inputDataManager == null) { Debug.LogError("HandCalibrationManager: InputDataManager not assigned!"); enabled = false; return; }
        if (handAnimator == null) { Debug.LogError("HandCalibrationManager: HandAnimator not assigned!"); enabled = false; return; }
        if (instructionText == null || nextStepButton == null || finishCalibrationButton == null || cancelCalibrationButton == null || startCalibrationButton == null)
        {
            Debug.LogError("HandCalibrationManager: One or more UI elements not assigned!");
            enabled = false; return;
        }
        
        DeactivateCalibrationUI(); 
        startCalibrationButton.onClick.AddListener(StartFullCalibrationProcess);
        nextStepButton.onClick.AddListener(OnNextStepButtonPressed);
        finishCalibrationButton.onClick.AddListener(ApplyAndSaveChanges);
        cancelCalibrationButton.onClick.AddListener(CancelCalibration);
    }

    private void ActivateCalibrationUI()
    {
        if(startCalibrationButton != null) startCalibrationButton.gameObject.SetActive(false); 
        if(nextStepButton != null) nextStepButton.gameObject.SetActive(true);
        if(finishCalibrationButton != null) finishCalibrationButton.gameObject.SetActive(false); 
        if(cancelCalibrationButton != null) cancelCalibrationButton.gameObject.SetActive(true);
        if (instructionText != null) instructionText.gameObject.SetActive(true);
        if (handAnimator != null) handAnimator.IsAnimationPaused = true; 
    }

    private void DeactivateCalibrationUI()
    {
        if(startCalibrationButton != null) startCalibrationButton.gameObject.SetActive(true);
        if(nextStepButton != null) nextStepButton.gameObject.SetActive(false);
        if(finishCalibrationButton != null) finishCalibrationButton.gameObject.SetActive(false);
        if(cancelCalibrationButton != null) cancelCalibrationButton.gameObject.SetActive(false);
        if (instructionText != null) {
            instructionText.text = "Calibration not active."; 
        }
        if (handAnimator != null) handAnimator.IsAnimationPaused = false; 
         _currentState = CalibrationState.Idle;
    }
    
    public void StartFullCalibrationProcess()
    {
        Debug.Log("Starting full calibration process...");
        for (int i = 0; i < NUM_FINGERS; i++) { 
            _tempMinPotValues[i] = (inputDataManager.RawPotValues.Length > i) ? inputDataManager.RawPotValues[i] : 4095; 
            _tempMaxPotValues[i] = (inputDataManager.RawPotValues.Length > i) ? inputDataManager.RawPotValues[i] : 0;
        }
        _currentFingerIndexToCalibrate = 0; 
        _currentState = CalibrationState.CalibratingFingerOpen;
        ActivateCalibrationUI();
        UpdateInstructionText();
        
        TMP_Text nextButtonText = nextStepButton.GetComponentInChildren<TMP_Text>();
        if (nextButtonText != null) nextButtonText.text = "Capture Open Pose";
    }

    private void UpdateInstructionText()
    {
        if (instructionText == null) return;
        string fingerName = (_currentFingerIndexToCalibrate < fingerNames.Length) ? fingerNames[_currentFingerIndexToCalibrate] : "Finger " + (_currentFingerIndexToCalibrate + 1) ;

        TMP_Text nextButtonText = null;
        if(nextStepButton != null) nextButtonText = nextStepButton.GetComponentInChildren<TMP_Text>();


        switch (_currentState)
        {
            case CalibrationState.CalibratingFingerOpen:
                instructionText.text = $"Hold your '{fingerName}' finger FULLY OPEN and relaxed.\nPress 'Capture Open Pose'.";
                if (nextButtonText != null) nextButtonText.text = "Capture Open Pose";
                break;
            case CalibrationState.CalibratingFingerClosed:
                instructionText.text = $"Now, make a tight FIST, curling your '{fingerName}' finger fully.\nPress 'Capture Closed Pose'.";
                if (nextButtonText != null) nextButtonText.text = "Capture Closed Pose";
                break;
            case CalibrationState.CalibratingNeutralPose:
                instructionText.text = "Hold your hand in a comfortable, FLAT, PALM-DOWN, FINGERS-FORWARD pose.\nPress 'Set Neutral Orientation'.";
                 if (nextButtonText != null) nextButtonText.text = "Set Neutral Orientation";
                break;
            case CalibrationState.Done:
                instructionText.text = "Calibration data captured! Press 'Apply & Save' or 'Cancel'.";
                if(nextStepButton != null) nextStepButton.gameObject.SetActive(false);
                if(finishCalibrationButton != null) finishCalibrationButton.gameObject.SetActive(true);
                break;
            default:
                instructionText.text = "Calibration Idle.";
                break;
        }
    }

    // --- METHOD WITH THE FIX ---
    public void OnNextStepButtonPressed()
    {
        if (inputDataManager == null || inputDataManager.RawPotValues == null)
        {
            Debug.LogError("InputDataManager is not ready.");
            return;
        }

        // Only check the finger index if we are in a finger calibration state
        bool isFingerCalibrationStep = (_currentState == CalibrationState.CalibratingFingerOpen || _currentState == CalibrationState.CalibratingFingerClosed);
        if (isFingerCalibrationStep && _currentFingerIndexToCalibrate >= inputDataManager.RawPotValues.Length)
        {
            Debug.LogError($"Finger index {_currentFingerIndexToCalibrate} is out of bounds. Cannot capture finger pose.");
            CancelCalibration(); // Cancel if we are in an invalid state
            return;
        }

        // Now, the rest of the switch statement can execute correctly
        switch (_currentState)
        {
            case CalibrationState.CalibratingFingerOpen:
                _tempMinPotValues[_currentFingerIndexToCalibrate] = inputDataManager.RawPotValues[_currentFingerIndexToCalibrate];
                Debug.Log($"Captured OPEN for {fingerNames[_currentFingerIndexToCalibrate]}: {_tempMinPotValues[_currentFingerIndexToCalibrate]}");
                _currentState = CalibrationState.CalibratingFingerClosed;
                break;

            case CalibrationState.CalibratingFingerClosed:
                _tempMaxPotValues[_currentFingerIndexToCalibrate] = inputDataManager.RawPotValues[_currentFingerIndexToCalibrate];
                 Debug.Log($"Captured CLOSED for {fingerNames[_currentFingerIndexToCalibrate]}: {_tempMaxPotValues[_currentFingerIndexToCalibrate]}");
                
                if (_tempMinPotValues[_currentFingerIndexToCalibrate] == _tempMaxPotValues[_currentFingerIndexToCalibrate]) {
                    Debug.LogWarning($"Min and Max values are identical for {fingerNames[_currentFingerIndexToCalibrate]}. Ensure potentiometer is moving.");
                } else if (_tempMinPotValues[_currentFingerIndexToCalibrate] > _tempMaxPotValues[_currentFingerIndexToCalibrate])
                {
                    int temp = _tempMinPotValues[_currentFingerIndexToCalibrate];
                    _tempMinPotValues[_currentFingerIndexToCalibrate] = _tempMaxPotValues[_currentFingerIndexToCalibrate];
                    _tempMaxPotValues[_currentFingerIndexToCalibrate] = temp;
                    Debug.LogWarning($"Min/Max swapped for {fingerNames[_currentFingerIndexToCalibrate]} to ensure Min < Max.");
                }
                
                _currentFingerIndexToCalibrate++;
                if (_currentFingerIndexToCalibrate < NUM_FINGERS) 
                {
                    _currentState = CalibrationState.CalibratingFingerOpen; 
                }
                else
                {
                    _currentState = CalibrationState.CalibratingNeutralPose; 
                }
                break;

            case CalibrationState.CalibratingNeutralPose:
                // This case will now be reached without error
                _tempNeutralOrientation = inputDataManager.TargetHandOrientation; 
                Debug.Log($"Captured NEUTRAL orientation: {_tempNeutralOrientation.eulerAngles}");
                _currentState = CalibrationState.Done;
                break;
        }
        UpdateInstructionText();
    }

    public void ApplyAndSaveChanges()
    {
        if (inputDataManager == null) return;

        Debug.Log("Applying and saving calibration data...");
        for (int i = 0; i < NUM_FINGERS; i++) 
        {
            if (_tempMaxPotValues[i] > _tempMinPotValues[i]) {
                inputDataManager.SetPotentiometerCalibrationData(i, _tempMinPotValues[i], _tempMaxPotValues[i]);
            } else {
                 Debug.LogWarning($"Skipping pot calibration for finger {fingerNames[i]} due to invalid range: Min={_tempMinPotValues[i]}, Max={_tempMaxPotValues[i]}");
            }
        }
        
        if (_currentState == CalibrationState.Done || _tempNeutralOrientation != default(Quaternion) ) 
        {
            inputDataManager.SetNeutralOrientation(_tempNeutralOrientation);
        } else {
            Debug.LogWarning("Neutral orientation was not captured in this calibration session. Previous offset (if any) remains.");
        }
        
        inputDataManager.SaveCalibrationSettings(); 
        
        if(instructionText!= null) instructionText.text = "Calibration Applied and Saved!";
        DeactivateCalibrationUI();
    }

    public void CancelCalibration()
    {
        Debug.Log("Calibration cancelled.");
        if(instructionText!= null) instructionText.text = "Calibration Cancelled.";
        DeactivateCalibrationUI();
    }
    
    public void ClearAllFingerCalibrationFromUI() {
        if(inputDataManager!=null) inputDataManager.ClearAllPotCalibration();
        if(instructionText!= null) instructionText.text = "All finger calibrations cleared.";
    }
    public void ClearOrientationCalibrationFromUI() {
        if(inputDataManager!=null) inputDataManager.ClearOrientationOffset();
        if(instructionText!= null) instructionText.text = "Orientation calibration cleared.";
    }
}