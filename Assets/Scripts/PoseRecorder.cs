// PoseRecorder.cs (V2.1 - Corrected Status Calls)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR // This special tag ensures editor-only code runs safely
using UnityEditor;
#endif

public class PoseRecorder : MonoBehaviour
{
    [Header("System Dependencies")]
    public InputDataManager rightHandInput;
    public InputDataManager leftHandInput;

    [Header("UI Elements")]
    public TMP_InputField poseNameInputField; 
    public TMP_Dropdown handSelectionDropdown; // 0=Right, 1=Left
    public Button recordButton;
    public TMP_Text statusText;

    private const string PoseSavePath = "Assets/Resources/Poses";

    void Start()
    {
        if(rightHandInput == null || leftHandInput == null)
        {
            Debug.LogError("PoseRecorder: Hand InputDataManager dependencies not set!");
            return;
        }

        recordButton.onClick.RemoveAllListeners();
        recordButton.onClick.AddListener(RecordCurrentPose);

        #if UNITY_EDITOR
        if (!AssetDatabase.IsValidFolder(PoseSavePath))
        {
            Debug.Log($"Creating pose save directory at: {PoseSavePath}");
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateFolder("Assets/Resources", "Poses");
        }
        #endif
    }

    public void RecordCurrentPose()
    {
        string poseName = poseNameInputField.text;
        if (string.IsNullOrWhiteSpace(poseName))
        {
            // MODIFIED: Changed 'true' to 'Color.red'
            SetStatus("Error: Pose name cannot be empty.", Color.red);
            return;
        }

        SetStatus($"Attempting to save pose '{poseName}'...", Color.yellow);

        string resourcePath = $"Poses/{poseName}";
        StaticPoseData targetPoseData = Resources.Load<StaticPoseData>(resourcePath);

        if (targetPoseData == null)
        {
            Debug.Log($"No existing pose found. Creating new asset for '{poseName}'.");
            targetPoseData = CreateNewPoseAsset(poseName);
            if (targetPoseData == null)
            {
                // MODIFIED: Changed 'true' to 'Color.red'
                SetStatus($"Error: Could not create new asset for '{poseName}'.", Color.red);
                return;
            }
        }

        int selectedHandIndex = handSelectionDropdown.value;
        InputDataManager sourceHand = (selectedHandIndex == 0) ? rightHandInput : leftHandInput;

        for (int i = 0; i < sourceHand.PotCurlTargets.Length; i++)
        {
            targetPoseData.fingerCurls[i] = sourceHand.PotCurlTargets[i];
        }

        #if UNITY_EDITOR
        EditorUtility.SetDirty(targetPoseData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        #endif

        SetStatus($"Successfully saved pose '{poseName}'!", Color.green);
    }

    private StaticPoseData CreateNewPoseAsset(string name)
    {
        #if UNITY_EDITOR
            StaticPoseData newPose = ScriptableObject.CreateInstance<StaticPoseData>();
            newPose.poseName = name;

            string fullPath = $"{PoseSavePath}/{name}.asset";
            
            AssetDatabase.CreateAsset(newPose, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"New pose asset created at {fullPath}");
            return newPose;
        #else
            Debug.LogError("Cannot create new poses in a built game. This is an editor-only tool.");
            return null;
        #endif
    }

    private void SetStatus(string message, Color color)
    {
        if(statusText == null) return;
        statusText.text = message;
        statusText.color = color;
    }
}