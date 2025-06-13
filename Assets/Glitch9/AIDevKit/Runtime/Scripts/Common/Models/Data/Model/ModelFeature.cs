using System;
using System.Collections.Generic;

namespace Glitch9.AIDevKit
{
    /// <summary>
    /// Formerly known as <see cref="ModelCapability"/>, this enum represents the features that a model can support.
    /// It is used to determine the capabilities of a model and to check if a specific feature is supported.
    /// </summary>
    [Flags]
    public enum ModelFeature
    {
        None = 0,

        // Core Language Capabilities
        TextGeneration = 1 << 0,
        FineTuning = 1 << 1,
        Streaming = 1 << 2,
        StructuredOutputs = 1 << 3,
        CodeExecution = 1 << 4,
        FunctionCalling = 1 << 5,
        Caching = 1 << 6,

        // Media Capabilities
        ImageGeneration = 1 << 7,
        ImageInpainting = 1 << 8,
        SpeechGeneration = 1 << 9,
        SpeechRecognition = 1 << 10,
        SoundFXGeneration = 1 << 11,
        VoiceChanger = 1 << 12,
        VideoGeneration = 1 << 13,

        // Other
        TextEmbedding = 1 << 14,
        Moderation = 1 << 15,
        Search = 1 << 16,
        Realtime = 1 << 17,
        ComputerUse = 1 << 18,

        // Added
        VoiceIsolation = 1 << 19,
    }

    internal static class ModelFeatureExtensions
    {
        private static readonly Dictionary<ModelFeature, string> _names = new()
        {
            { ModelFeature.TextGeneration, "Text Generation" },
            { ModelFeature.ImageGeneration, "Image Generation" },
            { ModelFeature.SpeechRecognition, "Speech Recognition" },
            { ModelFeature.SpeechGeneration, "Speech Generation" },
            { ModelFeature.Moderation, "Moderation" },
            { ModelFeature.FunctionCalling, "Function Calling" },
            { ModelFeature.Realtime, "Real-time" },
            { ModelFeature.FineTuning, "Fine Tuning" },
            { ModelFeature.Streaming, "Streaming" },
            { ModelFeature.Caching, "Caching" },
            { ModelFeature.ImageInpainting, "Image Inpainting" },
            { ModelFeature.SoundFXGeneration, "Sound FX Generation" },
            { ModelFeature.VideoGeneration, "Video Generation" },
            { ModelFeature.TextEmbedding, "Text Embedding" },
            { ModelFeature.Search, "Search" },
            { ModelFeature.ComputerUse, "Computer Use" },
            { ModelFeature.StructuredOutputs, "Structured Outputs" },
            { ModelFeature.CodeExecution, "Code Execution" }
        };

        internal static string GetName(this ModelFeature feature)
        {
            if (_names.TryGetValue(feature, out string name)) return name;
            return feature.ToString(); // Fallback to the enum name if not found
        }
    }
}