// HandAnimator.cs
using UnityEngine;

public class HandAnimator : MonoBehaviour
{
    [Header("Dependencies")]
    // MODIFIED: Assign the InputDataManager for the specific hand this animator controls.
    public InputDataManager inputDataManager;

    [Header("Hand Model References")]
    public Transform handRootTransform;
    public Finger[] fingers = new Finger[5];

    [Header("Movement Configuration")]
    public float fingerCurlMinAngle = 0f;
    public float fingerCurlMaxAngle = 90f;
    public float orientationSlerpSpeed = 15f;
    public float fingerSlerpSpeed = 20f;

    public bool IsAnimationPaused { get; set; } = false;

    void Start()
    {
        // Added a reference to the specific hand for clearer logs.
        if (inputDataManager == null) { Debug.LogError($"HandAnimator: InputDataManager not assigned!", this.gameObject); enabled = false; return; }
        if (handRootTransform == null) { Debug.LogError($"HandAnimator ({inputDataManager.handType}): Hand Root Transform not assigned!", this.gameObject); enabled = false; return; }
        if (fingers == null || fingers.Length != 5) { Debug.LogError($"HandAnimator ({inputDataManager.handType}): Fingers array not set up correctly!", this.gameObject); enabled = false; return; }
    }

    void Update()
    {
        if (IsAnimationPaused || !enabled || inputDataManager == null) return;

        // The core logic is the same, but it now gets data from its specific InputDataManager.
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