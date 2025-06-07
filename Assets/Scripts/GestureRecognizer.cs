// // GestureRecognizer.cs
// using UnityEngine;
// using Unity.Sentis; // The Sentis engine namespace
// using System;
// using System.Collections.Generic;
// using System.Linq; // For an easy way to get the last N elements

// public class GestureRecognizer : MonoBehaviour
// {
//     [Header("Dependencies")]
//     public InputDataManager inputDataManager; // Assign in Inspector

//     [Header("ML Model")]
//     public ModelAsset modelAsset; // Assign your .onnx model file here in Inspector
//     private Model _runtimeModel;
//     private IWorker _engine;

//     [Header("Real-time Recognition Settings")]
//     [Tooltip("The number of frames the model expects for a single gesture sequence.")]
//     public int sequenceLength = 60; // Example: 60 frames = 1-2 seconds at 30-60fps
    
//     [Tooltip("How often to run inference (e.g., run every 10 frames).")]
//     public int inferenceInterval = 10; 
    
//     [Tooltip("Minimum confidence from the model to be considered a valid recognition.")]
//     public float confidenceThreshold = 0.85f;

//     // This event will be fired when a gesture is recognized
//     public event Action<string> OnGestureRecognized; 

//     // A buffer to hold the most recent sensor data
//     private List<GestureFrame> liveDataBuffer = new List<GestureFrame>();
//     private float lastInferenceTime = 0f;
//     private string[] gestureLabels; // Will hold the names of your gestures, e.g., {"wave", "fist", "point"}

//     void Start()
//     {
//         if (inputDataManager == null) { Debug.LogError("GestureRecognizer: InputDataManager not assigned!"); enabled = false; return; }
//         if (modelAsset == null) { Debug.LogError("GestureRecognizer: ONNX Model Asset not assigned!"); enabled = false; return; }

//         // --- Sentis: Load the model and create the inference engine ---
//         _runtimeModel = ModelLoader.Load(modelAsset);
//         _engine = WorkerFactory.CreateWorker(BackendType.GPUCompute, _runtimeModel);

//         // --- IMPORTANT: Define your gesture labels ---
//         // The order MUST MATCH the output order of your trained model.
//         // For example, if your Python script trained with "fist" as class 0, "wave" as class 1, "point" as class 2...
//         gestureLabels = new string[] { "fist", "wave", "point" }; // ** REPLACE WITH YOUR ACTUAL GESTURE LABELS IN THE CORRECT ORDER **
//     }

//     void Update()
//     {
//         if (inputDataManager == null) return;

//         // --- 1. Continuously collect data into a buffer ---
//         // We create a new GestureFrame from the live data
//         liveDataBuffer.Add(new GestureFrame(
//             Time.time,
//             inputDataManager.TargetHandOrientation,
//             inputDataManager.PotCurlTargets
//         ));

//         // Keep the buffer at a fixed size (the sequence length our model expects)
//         while (liveDataBuffer.Count > sequenceLength)
//         {
//             liveDataBuffer.RemoveAt(0);
//         }

//         // --- 2. Run inference periodically ---
//         // Check if enough time has passed since the last inference run
//         if (Time.time - lastInferenceTime > (inferenceInterval * Time.deltaTime) && liveDataBuffer.Count == sequenceLength)
//         {
//             lastInferenceTime = Time.time;
//             RunInference();
//         }
//     }

//     private void RunInference()
//     {
//         // This is a placeholder for the logic to prepare data and run the model.
//         // The actual implementation will depend on your model's specific input shape.
//         Debug.Log("Running inference with a window of " + liveDataBuffer.Count + " frames.");

//         // --- Step A: Prepare the input Tensor ---
//         // You would convert your 'liveDataBuffer' (a list of GestureFrames) into a flat array or
//         // multi-dimensional array that matches your model's expected input shape (e.g., [1, sequenceLength, numFeatures]).
//         // let's say numFeatures is 9 (w,x,y,z for quat + 5 for fingers)
//         // TensorFloat inputTensor = new TensorFloat(new TensorShape(1, sequenceLength, 9), yourFlatArrayOfData);

//         // --- Step B: Execute the model ---
//         // _engine.Execute(inputTensor);

//         // --- Step C: Get the output Tensor ---
//         // TensorFloat outputTensor = _engine.PeekOutput() as TensorFloat;

//         // --- Step D: Interpret the results ---
//         // The outputTensor will contain probabilities for each gesture. Find the one with the highest probability.
//         // float highestConfidence = 0;
//         // int recognizedIndex = -1;
//         // for (int i = 0; i < gestureLabels.Length; i++) {
//         //    if (outputTensor[i] > highestConfidence) {
//         //        highestConfidence = outputTensor[i];
//         //        recognizedIndex = i;
//         //    }
//         // }

//         // --- Step E: Fire the event if confidence is high enough ---
//         // if (recognizedIndex != -1 && highestConfidence >= confidenceThreshold) {
//         //     string recognizedGesture = gestureLabels[recognizedIndex];
//         //     OnGestureRecognized?.Invoke(recognizedGesture);
//         //     Debug.Log($"RECOGNIZED: {recognizedGesture} with confidence {highestConfidence}");
//         //     // Optional: Clear the buffer to prevent immediate re-recognition
//         //     // liveDataBuffer.Clear(); 
//         // }

//         // --- IMPORTANT ---
//         // The commented-out code above is a template. You will need to implement the actual
//         // data-to-tensor conversion and result interpretation based on your specific trained model.
//     }

//     void OnDestroy()
//     {
//         // Clean up the Sentis engine when the object is destroyed
//         _engine?.Dispose();
//     }
// }