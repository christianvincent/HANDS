// StaticPoseData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Static Pose", menuName = "Gesture Recognition/Static Pose Data")]
public class StaticPoseData : ScriptableObject
{
    [Tooltip("The name of the pose (e.g., 'Thumbs Up', 'Point').")]
    public string poseName;

    [Tooltip("The target finger curl values for this pose (0.0 = open, 1.0 = closed).")]
    public List<float> fingerCurls = new List<float>(new float[5]);
}