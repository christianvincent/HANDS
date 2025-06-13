using System;
using Glitch9.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Glitch9.AIDevKit
{
    /// <summary>
    /// Usage statistics for the completion request. 
    /// </summary>
    [Serializable]
    public class Usage
    {
        // static method to create empty
        public static Usage Empty() => new();
        public ReferencedDictionary<UsageType, int> usages = new();

        /// <summary> 
        /// Number of tokens in the prompt.
        /// </summary>
        public int? InputTokens
        {
            get => usages.TryGetValue(UsageType.InputToken, out var input) ? input : null;
            //set => usages[UsageType.InputToken] = value ?? 0;
            set
            {
                if (usages.ContainsKey(UsageType.InputToken))
                {
                    usages[UsageType.InputToken] = value ?? 0;
                }
                else
                {
                    usages.Add(UsageType.InputToken, value ?? 0);
                }
            }
        }

        /// <summary> 
        /// Number of tokens in the generated completion.
        /// </summary> 
        public int? OutputTokens
        {
            get => usages.TryGetValue(UsageType.OutputToken, out var output) ? output : null;
            //set => usages[UsageType.OutputToken] = value ?? 0;
            set
            {
                if (usages.ContainsKey(UsageType.OutputToken))
                {
                    usages[UsageType.OutputToken] = value ?? 0;
                }
                else
                {
                    usages.Add(UsageType.OutputToken, value ?? 0);
                }
            }
        }

        public bool IsEmpty => usages.IsNullOrEmpty();
        public bool IsFree => usages.ContainsKey(UsageType.Free);

        public Usage() { }
        public Usage(UsageType type, int count) => usages[type] = count;
        public static Usage Free() => new() { usages = { [UsageType.Free] = 0 } };
        public static Usage PerMinute(double cost) => new() { usages = { [UsageType.PerMinute] = (int)cost } };
        public static Usage PerCharacter(double cost) => new() { usages = { [UsageType.PerCharacter] = (int)cost } };


        private string _displayText;
        public override string ToString()
        {
            if (_displayText != null) return _displayText;

            if (usages.IsNullOrEmpty())
            {
                _displayText = "N/A";
                return _displayText;
            }

            using (StringBuilderPool.Get(out var sb))
            {
                foreach (var kvp in usages)
                {
                    if (kvp.Value > 0)
                    {
                        sb.Append($"{kvp.Key.GetInspectorName()}: {kvp.Value}, ");
                    }
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 2, 2); // remove last comma and space
                }
                else
                {
                    sb.Append("N/A");
                }

                _displayText = sb.ToString();
                return _displayText;
            }
        }
    }

    public static class UsageExtensions
    {
        public static Usage Merge(this Usage usage, Usage usageToAdd)
        {
            if (usage == null) return usageToAdd;
            if (usageToAdd == null || usageToAdd.IsEmpty) return usage;

            usage.InputTokens += usageToAdd.InputTokens;
            usage.OutputTokens += usageToAdd.OutputTokens;

            if (usageToAdd.usages.IsNotNullOrEmpty())
            {
                usage.usages ??= new();

                foreach (var kvp in usageToAdd.usages)
                {
                    if (usage.usages.ContainsKey(kvp.Key))
                    {
                        usage.usages[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        usage.usages.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            return usage;
        }
    }

    public class UsageConverter : JsonConverter<Usage>
    {
        private readonly Api _api;
        public UsageConverter(Api api) => _api = api;

        public override Usage ReadJson(JsonReader reader, Type objectType, Usage existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject json = JObject.Load(reader);

            if (_api == Api.Google)
            {
                return new Usage
                {
                    InputTokens = (int?)json["promptTokenCount"],
                    OutputTokens = (int?)json["candidatesTokenCount"],
                    //TotalTokens = (int?)json["totalTokenCount"]
                };
            }

            return new Usage
            {
                InputTokens = (int?)json["prompt_tokens"],
                OutputTokens = (int?)json["completion_tokens"],
                //TotalTokens = (int?)json["total_tokens"]
            };
        }

        public override void WriteJson(JsonWriter writer, Usage value, JsonSerializer serializer)
        {
            throw new NotImplementedException("WriteJson is not implemented(no need) for UsageConverter.");
        }
    }


    public enum UsageType
    {
        [InspectorName("Free")] Free,

        [InspectorName("Input Token")] InputToken,
        [InspectorName("Output Token")] OutputToken,
        [InspectorName("Training Token")] TrainingToken,
        [InspectorName("Cached Input Token")] CachedInputToken,

        [InspectorName("Image (general)")] Image,
        [InspectorName("Image SD 256×256")] ImageSD256,
        [InspectorName("Image SD 512×512")] ImageSD512,
        [InspectorName("Image SD 1024×1024")] ImageSD1024,
        [InspectorName("Image SD 1024×1792, 1792x1024")] ImageSD1792,
        [InspectorName("Image HD 1024×1024")] ImageHD1024,
        [InspectorName("Image HD 1792×1024, 1792x1024")] ImageHD1792,
        [InspectorName("Image Low 1024×1024")] ImageLow1024,
        [InspectorName("Image Low 1024×1536, 1536×1024")] ImageLow1536,
        [InspectorName("Image Medium 1024×1024")] ImageMedium1024,
        [InspectorName("Image Medium 1024×1536, 1536×1024")] ImageMedium1536,
        [InspectorName("Image High 1024×1024")] ImageHigh1024,
        [InspectorName("Image High 1024×1536, 1536×1024")] ImageHigh1536,

        [InspectorName("Per Minute")] PerMinute,
        [InspectorName("Per Character")] PerCharacter,
        [InspectorName("Per Request")] PerRequest,

        [InspectorName("Input Cache Read")] InputCacheRead,
        [InspectorName("Input Cache Write")] InputCacheWrite,

        [InspectorName("Per Web Search (low)")] WebSearchLow,
        [InspectorName("Per Web Search (medium)")] WebSearch,
        [InspectorName("Per Web Search (high)")] WebSearchHigh,
        [InspectorName("Internal Reasoning")] InternalReasoning,

        [InspectorName("Output Token (thinking)")] OutputTokenThinking,
    }

    public static class UsageTypeExtensions
    {
        public static bool IsTokenType(this UsageType type)
        {
            return type == UsageType.InputToken
            || type == UsageType.OutputToken
            || type == UsageType.TrainingToken
            || type == UsageType.CachedInputToken;
        }
    }
}