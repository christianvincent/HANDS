// HandAnimator.cs
using UnityEngine;
using System;

public class HandAnimator : MonoBehaviour
{
    [Header("Dependencies")]
    public InputDataManager inputDataManager;

    [Header("Hand Model References")]
    public Transform handRootTransform;
    public Finger[] fingers = new Finger[5];

    [Header("Movement Configuration")]
    public float fingerCurlMinAngle = 0f;
    public float fingerCurlMaxAngle = 90f;
    public float orientationSlerpSpeed = 10f;
    public float fingerSlerpSpeed = 15f;

    // --- NEW: For pausing animation during calibration ---
    public bool IsAnimationPaused { get; set; } = false;
    // --- END NEW ---

    void Start()
    {
        if (inputDataManager == null) { Debug.LogError("HandAnimator: InputDataManager not assigned!"); enabled = false; return; }
        if (handRootTransform == null) { Debug.LogError("HandAnimator: Hand Root Transform not assigned!"); enabled = false; return; }
        if (fingers == null || fingers.Length != 5) { Debug.LogError("HandAnimator: Fingers array not set up correctly!"); if (fingers == null || fingers.Length !=5) fingers = new Finger[5]; for(int i=0; i<5; i++) if(fingers[i]==null) fingers[i] = new Finger { name = "Finger " + i }; enabled = false; return; }
    }

    void Update()
    {
        // --- NEW: Check if animation is paused ---
        if (IsAnimationPaused || !enabled || inputDataManager == null) return;
        // --- END NEW ---

        Quaternion currentTargetOrientation = inputDataManager.TargetHandOrientation;
        float[] currentPotCurls = inputDataManager.PotCurlTargets;

        if (handRootTransform != null)
        {
            handRootTransform.localRotation = Quaternion.Slerp(handRootTransform.localRotation, currentTargetOrientation, Time.deltaTime * orientationSlerpSpeed);
        }

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
        Quaternion targetLocalRotation = Quaternion.Euler(targetAngleForThisJoint, 0, 0); // Assumes X-axis curl
        joint.localRotation = Quaternion.Slerp(joint.localRotation, targetLocalRotation, Time.deltaTime * fingerSlerpSpeed);
    }
}