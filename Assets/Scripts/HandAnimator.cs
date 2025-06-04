// HandAnimator.cs
using UnityEngine;
using System; // For Array.Copy if needed, though direct access is fine

// Note: Finger class should be defined in your DataTypes.cs file

public class HandAnimator : MonoBehaviour
{
    [Header("Dependencies")]
    public InputDataManager inputDataManager; // Assign your InputDataManager GameObject/Component

    [Header("Hand Model References")]
    public Transform handRootTransform;       // Assign the main transform of your hand model
    public Finger[] fingers = new Finger[5];  // Assign finger joint configurations in Inspector

    [Header("Movement Configuration")]
    public float fingerCurlMinAngle = 0f;
    public float fingerCurlMaxAngle = 90f;
    public float orientationSlerpSpeed = 10f;
    public float fingerSlerpSpeed = 15f;

    void Start()
    {
        if (inputDataManager == null)
        {
            Debug.LogError("HandAnimator: InputDataManager not assigned in Inspector! Animation will not work.");
            enabled = false; // Disable this script if dependency is missing
            return;
        }
        if (handRootTransform == null)
        {
            Debug.LogError("HandAnimator: Hand Root Transform not assigned!");
            enabled = false;
            return;
        }
        if (fingers == null || fingers.Length != 5)
        {
            Debug.LogError("HandAnimator: Fingers array not set up correctly (must be size 5).");
            // Basic init to prevent further null errors if script is not disabled by above checks
            if (fingers == null || fingers.Length !=5) fingers = new Finger[5];
            for(int i=0; i<5; i++) if(fingers[i]==null) fingers[i] = new Finger { name = "Finger " + i };
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (!enabled || inputDataManager == null) return;

        // Get processed data from InputDataManager
        Quaternion currentTargetOrientation = inputDataManager.TargetHandOrientation;
        float[] currentPotCurls = inputDataManager.PotCurlTargets;

        // Apply orientation to hand root
        if (handRootTransform != null)
        {
            handRootTransform.localRotation = Quaternion.Slerp(
                handRootTransform.localRotation, 
                currentTargetOrientation, 
                Time.deltaTime * orientationSlerpSpeed
            );
        }

        // Apply multi-joint finger curls
        for (int i = 0; i < fingers.Length; i++)
        {
            if (fingers[i] == null || currentPotCurls == null || i >= currentPotCurls.Length) continue;
            
            float potCurlValue = currentPotCurls[i];

            ApplyCurlToJoint(fingers[i].proximalJoint, potCurlValue, fingers[i].proximalCurlWeight);
            ApplyCurlToJoint(fingers[i].intermediateJoint, potCurlValue, fingers[i].intermediateCurlWeight);
            ApplyCurlToJoint(fingers[i].distalJoint, potCurlValue, fingers[i].distalCurlWeight);
        }
    }

    private void ApplyCurlToJoint(Transform joint, float potCurlValue, float weight)
    {
        if (joint == null) return;

        float targetAngleForThisJoint = Mathf.Lerp(fingerCurlMinAngle, fingerCurlMaxAngle, potCurlValue * weight);
        
        // IMPORTANT: Ensure this Euler angle order and axis matches your model's rigging for a curl.
        // This example assumes curling around the local X-axis.
        Quaternion targetLocalRotation = Quaternion.Euler(targetAngleForThisJoint, 0, 0);
        // If it's local Z-axis: Quaternion targetLocalRotation = Quaternion.Euler(0, 0, targetAngleForThisJoint);
        // If it's local Y-axis: Quaternion targetLocalRotation = Quaternion.Euler(0, targetAngleForThisJoint, 0);
        // You might need to negate targetAngleForThisJoint if it curls the wrong way.

        joint.localRotation = Quaternion.Slerp(
            joint.localRotation, 
            targetLocalRotation, 
            Time.deltaTime * fingerSlerpSpeed
        );
    }
}