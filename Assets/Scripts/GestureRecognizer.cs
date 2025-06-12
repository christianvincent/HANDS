// GestureRecognizer.cs
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GestureRecognizer : MonoBehaviour
{
    [Header("System Dependencies")]
    [Tooltip("The InputDataManager for the hand you want to track (e.g., RightHandController).")]
    public InputDataManager handToTrack;

    [Header("Recognition Settings")]
    [Tooltip("The maximum difference score to be considered a match. Lower is stricter. Start with 0.3")]
    public float recognitionThreshold = 0.3f;

    [Header("UI Elements")]
    [Tooltip("The TextMeshPro UI element to display the name of the recognized pose.")]
    public TextMeshProUGUI recognizedPoseText;

    // The internal library of poses loaded from Resources
    private List<StaticPoseData> _poseLibrary;
    private StaticPoseData _lastRecognizedPose = null;

    void Start()
    {
        if (handToTrack == null)
        {
            Debug.LogError("GestureRecognizer: 'Hand To Track' (InputDataManager) is not assigned!");
            enabled = false;
            return;
        }

        // Load all pose assets from the Resources/Poses folder
        LoadPoseLibrary();

        if (recognizedPoseText != null)
        {
            recognizedPoseText.text = "No Pose Detected";
        }
    }

    void LoadPoseLibrary()
    {
        _poseLibrary = Resources.LoadAll<StaticPoseData>("Poses").ToList();
        if (_poseLibrary.Count == 0)
        {
            Debug.LogError("GestureRecognizer: No StaticPoseData assets found in 'Assets/Resources/Poses' folder!");
        }
        else
        {
            Debug.Log($"Loaded {_poseLibrary.Count} poses into the library.");
        }
    }

    void Update()
    {
        if (_poseLibrary.Count == 0) return;

        // Get the current finger curls from the hand we are tracking
        float[] currentCurls = handToTrack.PotCurlTargets;

        StaticPoseData bestMatch = null;
        float lowestDifference = float.MaxValue;

        // Loop through every pose in our library
        foreach (var pose in _poseLibrary)
        {
            // Calculate how different the current pose is from the template
            float difference = CalculatePoseDifference(currentCurls, pose.fingerCurls);

            // If this pose is a better match than the last one we checked, store it
            if (difference < lowestDifference)
            {
                lowestDifference = difference;
                bestMatch = pose;
            }
        }

        // After checking all poses, if our best match is within the threshold...
        if (bestMatch != null && lowestDifference < recognitionThreshold)
        {
            // We have a recognized pose!
            if (bestMatch != _lastRecognizedPose)
            {
                _lastRecognizedPose = bestMatch;
                if (recognizedPoseText != null)
                {
                    recognizedPoseText.text = _lastRecognizedPose.poseName;
                    Debug.Log($"Recognized Pose: {_lastRecognizedPose.poseName} (Score: {lowestDifference})");
                }
            }
        }
        else
        {
            // No pose is close enough to be a match
            if (_lastRecognizedPose != null)
            {
                _lastRecognizedPose = null;
                if (recognizedPoseText != null)
                {
                    recognizedPoseText.text = "No Pose Detected";
                }
            }
        }
    }

    // This helper function calculates the "difference score" between two poses.
    // A lower score means the poses are more similar.
    private float CalculatePoseDifference(float[] liveCurls, List<float> templateCurls)
    {
        float totalDifference = 0;
        for (int i = 0; i < 5; i++)
        {
            totalDifference += Mathf.Abs(liveCurls[i] - templateCurls[i]);
        }
        return totalDifference;
    }
}