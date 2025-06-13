using System;
using System.Collections.Generic;
using Glitch9.Editor;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    internal static class AIDevKitGUIUtility
    {
        private static Dictionary<ModelFeature, GUIContent> _capContents;

        private static readonly ModelFeature[] _priorityOrder = new[]
        {
            ModelFeature.TextGeneration,
            ModelFeature.Realtime,
            ModelFeature.ImageGeneration,
            ModelFeature.SpeechRecognition,
            ModelFeature.SpeechGeneration,
            ModelFeature.Moderation,
            ModelFeature.FunctionCalling,
            ModelFeature.Streaming,
            ModelFeature.Caching,
            ModelFeature.ImageInpainting,
            ModelFeature.SoundFXGeneration,
            ModelFeature.VideoGeneration,
            ModelFeature.TextEmbedding,
            ModelFeature.Search,
            ModelFeature.ComputerUse,
            ModelFeature.StructuredOutputs,
            ModelFeature.FineTuning,
        };

        internal static readonly CurrencyCode[] SelectedCurrencyCodes = new[]
        {
            CurrencyCode.USD,
            CurrencyCode.EUR,
            CurrencyCode.GBP,
            CurrencyCode.CAD,
            CurrencyCode.AUD,
            CurrencyCode.JPY,
            CurrencyCode.CNY,
            CurrencyCode.INR,
            CurrencyCode.RUB,
            CurrencyCode.BRL,
            CurrencyCode.KRW,
        };

        private static Dictionary<ModelFeature, GUIContent> CreateCapabilityContents()
        {
            Dictionary<ModelFeature, GUIContent> contents = new();
            var array = Enum.GetValues(typeof(ModelFeature));
            // Sort the array based on the priority order
            List<ModelFeature> sortedArray = new(array.Length);

            foreach (ModelFeature capability in array)
            {
                if (capability == ModelFeature.None) continue;
                sortedArray.Add(capability);
            }

            sortedArray.Sort((x, y) =>
            {
                int indexX = Array.IndexOf(_priorityOrder, x);
                int indexY = Array.IndexOf(_priorityOrder, y);
                return indexX.CompareTo(indexY);
            });

            foreach (ModelFeature cap in sortedArray)
            {
                if (cap == ModelFeature.None) continue;

                GUIContent content = new(GetModelFeatureIcon(cap), cap.GetName());
                contents.Add(cap, content);
            }

            return contents;
        }

        internal static List<GUIContent> GetFeatureContents(ModelFeature cap)
        {
            List<GUIContent> contents = new();

            if (cap == ModelFeature.None)
            {
                contents.Add(new GUIContent("None"));
                return contents;
            }

            _capContents ??= CreateCapabilityContents();

            long capValue = (long)cap;
            foreach (var kvp in _capContents)
            {
                long bit = (long)kvp.Key;
                if ((capValue & bit) == bit)
                {
                    contents.Add(kvp.Value);
                }
            }

            return contents;
        }

        internal static Texture GetModelFeatureIcon(ModelFeature cap)
        {
            return cap switch
            {
                ModelFeature.TextGeneration => AIDevKitIcons.Text,
                ModelFeature.StructuredOutputs => AIDevKitIcons.JsonSchema,
                ModelFeature.CodeExecution => AIDevKitIcons.Code,
                ModelFeature.FunctionCalling => AIDevKitIcons.Tool,
                ModelFeature.Caching => AIDevKitIcons.Caching,
                ModelFeature.ImageGeneration => AIDevKitIcons.Image,
                ModelFeature.ImageInpainting => AIDevKitIcons.Inpainting,
                ModelFeature.SpeechGeneration => AIDevKitIcons.TextToSpeech,
                ModelFeature.SpeechRecognition => AIDevKitIcons.SpeechToText,
                ModelFeature.SoundFXGeneration => AIDevKitIcons.SoundFX,
                ModelFeature.VideoGeneration => AIDevKitIcons.Video,
                ModelFeature.TextEmbedding => AIDevKitIcons.Embedding,
                ModelFeature.Moderation => AIDevKitIcons.Moderation,
                ModelFeature.Search => EditorIcons.Search,
                ModelFeature.Realtime => AIDevKitIcons.Realtime,
                ModelFeature.FineTuning => AIDevKitIcons.FineTuning,
                ModelFeature.Streaming => AIDevKitIcons.Streaming,
                ModelFeature.ComputerUse => AIDevKitIcons.Code,

                _ => EditorIcons.Question,
            };
        }

        internal static Texture2D GetApiIcon(Api api)
        {
            Texture2D texture = api switch
            {
                Api.OpenAI => AIDevKitIcons.OpenAI,
                Api.Google => AIDevKitIcons.Google,
                Api.ElevenLabs => AIDevKitIcons.ElevenLabs,
                Api.Mubert => AIDevKitIcons.Mubert,
                Api.Ollama => AIDevKitIcons.Ollama,
                Api.OpenRouter => AIDevKitIcons.OpenRouter,
                _ => AIDevKitIcons.OpenAI
            };

            return texture;
        }

        internal static string FormatValue<T>(T value)
        {
            return value switch
            {
                Enum e => e.GetInspectorName(),
                bool b => b ? "True" : "False",
                _ => value.ToString()
            };
        }
    }
}