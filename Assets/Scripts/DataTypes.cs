// DataType.cs
using UnityEngine;
using System.Collections.Generic;
using System;

// NEW: An enum to clearly identify which hand a component or data belongs to.
[System.Serializable]
public enum HandType { Right, Left }

// NEW: This class matches the combined JSON packet sent by the ESP32.
[System.Serializable]
public class Esp32CombinedData
{
    public List<float> q; // for the quaternion
    public List<int> p;   // for the potentiometers
}

// MODIFIED: This struct now holds the pose for a single hand.
[System.Serializable]
public struct HandPose
{
    public bool isTracked;
    public Quaternion handOrientation;
    public float[] fingerCurls;

    public HandPose(bool tracked, Quaternion orientation, float[] curls)
    {
        isTracked = tracked;
        handOrientation = orientation;
        if (curls != null)
        {
            fingerCurls = new float[curls.Length];
            Array.Copy(curls, fingerCurls, curls.Length);
        }
        else
        {
            fingerCurls = new float[5]; // Default to 5 fingers
        }
    }
}


// MODIFIED: A frame of a gesture now contains pose data for both hands.
[System.Serializable]
public struct GestureFrame
{
    public float timestamp;
    public HandPose rightHand;
    public HandPose leftHand;

    public GestureFrame(float time, HandPose right, HandPose left)
    {
        timestamp = time;
        rightHand = right;
        leftHand = left;
    }
}

// Unchanged, but kept for clarity
[System.Serializable]
public class GestureData
{
    public string gestureName;
    public List<GestureFrame> frames;
    public float totalDuration;

    public GestureData(string name)
    {
        gestureName = name;
        frames = new List<GestureFrame>();
        totalDuration = 0f;
    }
}

// Unchanged, but kept for clarity
[System.Serializable]
public class Finger
{
    public string name;
    public Transform proximalJoint;
    public Transform intermediateJoint;
    public Transform distalJoint;

    public float proximalCurlWeight = 1.0f;
    public float intermediateCurlWeight = 1.0f;
    public float distalCurlWeight = 0.8f;
}

// DEPRECATED: These classes are no longer needed with the new combined JSON format.
// You can safely delete them if you wish.
/*
[System.Serializable]
public class Esp32JsonData { public string key; }
[System.Serializable]
public class OrientationPayload { public string key; public List<float> value; }
[System.Serializable]
public class PotsPayload { public string key; public List<int> value; }
*/