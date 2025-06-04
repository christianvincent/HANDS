// DataTypes.cs
using UnityEngine;
using System.Collections.Generic; // For List
using System; // For Array.Copy and Serializable

// Helper struct for a single frame of gesture data
[System.Serializable]
public struct GestureFrame
{
    public float timestamp;
    public Quaternion handOrientation;
    public float[] fingerCurls; // Should be size 5

    public GestureFrame(float time, Quaternion orientation, float[] curls)
    {
        timestamp = time;
        handOrientation = orientation;
        fingerCurls = new float[curls.Length];
        Array.Copy(curls, fingerCurls, curls.Length);
    }
}

// Helper class to hold a complete gesture sequence
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

// Helper classes for parsing ESP32 JSON data
[System.Serializable]
public class Esp32JsonData { public string key; }

[System.Serializable]
public class OrientationPayload { public string key; public List<float> value; }

[System.Serializable]
public class PotsPayload { public string key; public List<int> value; }

// Helper class for managing finger joints
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