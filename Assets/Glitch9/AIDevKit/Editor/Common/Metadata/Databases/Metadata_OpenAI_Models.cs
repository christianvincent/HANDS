using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Glitch9.AIDevKit.Editor
{
    internal static class Metadata_OpenAI_Models
    {
        internal static ModelCatalogueEntry Resolve(ModelCatalogueEntry entry)
        {
            // Missing Properties: 
            // ✓ Capability
            // ✓ Name
            // ✘ Version 
            // ✓ Description 
            // ✓ InputModality, OutputModality 
            // ✘ InputTokenLimit, OutputTokenLimit 
            // ✓ IsFineTuned, BaseId

            entry.IsFineTuned = IsFineTuned(entry.Id);

            if (entry.IsFineTuned)
            {
                entry.BaseId = ResolveBaseId(entry.Id);
                entry.Name = ResolveFineTunedName(entry.Id);
                entry.Version = ResolveFineTunedVersion(entry.Id);
            }
            else
            {
                entry.Name = ModelNameResolver.ResolveFromId(entry.Id);
                entry.Description = GetDescription(entry.Id);
                entry.SetPrices(GetPrice(entry.Id));
            }

            if (TryGetModalityCapability(entry.Id, out (Modality input, Modality output, ModelFeature cap) result))
            {
                entry.InputModality = result.input;
                entry.OutputModality = result.output;
                entry.Capability = result.cap;
            }

            return entry;
        }

        internal static ModelPrice[] GetPrice(string modelId)
        {
            return Prices.kPrices.GetBestMatch(modelId, (notFound)
                => AIDevKitDebug.LogWarning($"Model ID '{notFound}' not found in OpenAI model prices."));
        }

        internal static string GetDescription(string modelId)
        {
            return Descriptions.kDescriptions.GetBestMatch(modelId, ',', (notFound)
                => AIDevKitDebug.LogWarning($"Model ID '{notFound}' not found in OpenAI model descriptions."));
        }

        internal static bool TryGetModalityCapability(string id, out (Modality input, Modality output, ModelFeature cap) result)
        {
            foreach (var kvp in ModalityCapability.ModelPatterns)
            {
                if (id.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    result = kvp.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }


        #region Prices
        internal static class Prices
        {
            internal static readonly Dictionary<string, ModelPrice[]> kPrices = new()
            { 
                // legacy models
                { "babbage-002", ModelPrice.PerInputOutput(0.0000004, 0.0000004) },
                { "davinci-002", ModelPrice.PerInputOutput(0.000002, 0.000002) },

                // not legacy, but old models
                { "gpt-3.5-turbo-1106", ModelPrice.PerInputOutput(0.000001, 0.000002) },
                { "gpt-3.5-turbo-0613", ModelPrice.PerInputOutput(0.0000015, 0.000002) },
                { "gpt-3.5-0301", ModelPrice.PerInputOutput(0.0000015, 0.000002) },
                { "gpt-3.5-turbo-instruct", ModelPrice.PerInputOutput(0.0000015, 0.000002) },
                { "gpt-3.5-turbo-16k", ModelPrice.PerInputOutput(0.000003, 0.000004) },
                { "gpt-3.5-turbo", ModelPrice.PerInputOutput(0.0000005, 0.0000015) },

                { "gpt-4-0125-preview", ModelPrice.PerInputOutput(0.00001, 0.00003) },
                { "gpt-4-1106-preview", ModelPrice.PerInputOutput(0.00001, 0.00003) },
                { "gpt-4-1106-vision-preview", ModelPrice.PerInputOutput(0.00001, 0.00003) },
                { "gpt-4-turbo", ModelPrice.PerInputOutput(0.00001, 0.00003) },

                { "chatgpt-4o-latest", ModelPrice.PerInputOutput(0.000005, 0.000015) }, 

                // new models      
                { "gpt-4.5-preview", ModelPrice.PerInputCachedInputOutput(0.000075, 0.0000375, 0.00015) },

                { "gpt-4.1-nano", ModelPrice.PerInputCachedInputOutput(0.0000001, 0.000000025, 0.0000004) },
                { "gpt-4.1-mini", new ModelPrice[] {
                    new (UsageType.InputToken, 0.0000004),
                    new (UsageType.CachedInputToken, 0.0000001),
                    new (UsageType.OutputToken, 0.0000016),
                    new (UsageType.WebSearchLow, 0.025),
                    new (UsageType.WebSearch, 0.0275),
                    new (UsageType.WebSearchHigh, 0.03),
                }},

                { "gpt-4.1", new ModelPrice[] {
                    new (UsageType.InputToken, 0.000002),
                    new (UsageType.CachedInputToken, 0.0000005),
                    new (UsageType.OutputToken, 0.000008),
                    new (UsageType.WebSearchLow, 0.03),
                    new (UsageType.WebSearch, 0.035),
                    new (UsageType.WebSearchHigh, 0.05),
                }},

                { "o4-mini", ModelPrice.PerInputCachedInputOutput(0.0000011, 0.000000275, 0.0000044) },

                { "o3-mini", ModelPrice.PerInputCachedInputOutput(0.0000011, 0.00000055, 0.0000044) },
                { "o3", ModelPrice.PerInputCachedInputOutput(0.00001, 0.0000025, 0.00004) },

                { "o1-pro", ModelPrice.PerInputOutput(0.00015, 0.0006) },
                { "o1-mini", ModelPrice.PerInputCachedInputOutput(0.0000011, 0.00000055, 0.0000044) },
                { "o1", ModelPrice.PerInputCachedInputOutput(0.000015, 0.0000075, 0.00006) },

                { "computer-use-preview", ModelPrice.PerInputOutput(0.000003, 0.000012) }, 

                // image models
                { "dall-e-2", new ModelPrice[] {
                    new (UsageType.ImageSD256, 0.016),
                    new (UsageType.ImageSD512, 0.018),
                    new (UsageType.ImageSD1024, 0.02),
                }},
                { "dall-e-3", new ModelPrice[] {
                    new (UsageType.ImageSD1024, 0.04),
                    new (UsageType.ImageSD1792, 0.08),
                    new (UsageType.ImageHD1024, 0.08),
                    new (UsageType.ImageHD1792, 0.12),
                }},
                { "gpt-image-1", new ModelPrice[] {
                    new (UsageType.ImageLow1024, 0.011),
                    new (UsageType.ImageLow1536, 0.016),
                    new (UsageType.ImageMedium1024, 0.042),
                    new (UsageType.ImageMedium1536, 0.063),
                    new (UsageType.ImageHigh1024, 0.167),
                    new (UsageType.ImageHigh1536, 0.25),
                }},
                 
                // embedding models
                { "text-embedding-3-small", ModelPrice.PerInputToken(0.00000002) },
                { "text-embedding-3-large", ModelPrice.PerInputToken(0.00000013) },
                { "text-embedding-ada-002", ModelPrice.PerInputToken(0.0000001) },

                // moderation models
                { "omni-moderation", ModelPrice.Free() },
                { "text-moderation", ModelPrice.Free() },
 
                // tts models 
                { "tts-1", ModelPrice.PerCharacter(0.000015) },
                { "tts-1-hd", ModelPrice.PerCharacter(0.00003) },  

                // stt models 
                { "whisper-1", ModelPrice.PerMinute(0.006) },

                { "gpt-4o-mini-audio-preview", ModelPrice.PerInputOutput(0.00000015, 0.0000006) },
                { "gpt-4o-mini-realtime-preview", ModelPrice.PerInputCachedInputOutput(0.0000006, 0.0000003, 0.0000024) },
                { "gpt-4o-mini-transcribe", ModelPrice.PerMinute(0.003, true) },
                { "gpt-4o-mini-tts", ModelPrice.PerMinute(0.015, true) },
                //{ "gpt-4o-mini-search-preview", ModelPrice.PerInputOutput(0.00000015, 0.0000006) },
                //{ "gpt-4o-mini", ModelPrice.PerInputCachedInputOutput(0.00000015, 0.000000075, 0.0000006) },
                { "gpt-4o-mini-search-preview", new ModelPrice[] {
                    new (UsageType.InputToken, 0.00000015),
                    new (UsageType.OutputToken, 0.0000006),
                    new (UsageType.WebSearchLow, 0.025),
                    new (UsageType.WebSearch, 0.0275),
                    new (UsageType.WebSearchHigh, 0.03),
                }},
                { "gpt-4o-mini", new ModelPrice[] {
                    new (UsageType.InputToken, 0.00000015),
                    new (UsageType.CachedInputToken, 0.000000075),
                    new (UsageType.OutputToken, 0.0000006),
                    new (UsageType.WebSearchLow, 0.025),
                    new (UsageType.WebSearch, 0.0275),
                    new (UsageType.WebSearchHigh, 0.03),
                }},

                { "gpt-4o-audio-preview", ModelPrice.PerInputOutput(0.0000025, 0.00001) },
                { "gpt-4o-realtime-preview", ModelPrice.PerInputCachedInputOutput(0.000005, 0.0000025, 0.00002) },
                { "gpt-4o-transcribe", ModelPrice.PerMinute(0.006, true) },
                { "gpt-4o-search-preview", new ModelPrice[] {
                    new (UsageType.InputToken, 0.0000025),
                    new (UsageType.OutputToken, 0.00001),
                    new (UsageType.WebSearchLow, 0.03),
                    new (UsageType.WebSearch, 0.035),
                    new (UsageType.WebSearchHigh, 0.05),
                }},
                { "gpt-4o", new ModelPrice[] {
                    new (UsageType.InputToken, 0.0000025),
                    new (UsageType.CachedInputToken, 0.00000125),
                    new (UsageType.OutputToken, 0.00001),
                    new (UsageType.WebSearchLow, 0.03),
                    new (UsageType.WebSearch, 0.035),
                    new (UsageType.WebSearchHigh, 0.05),
                }},

                { "gpt-4o-mini-audio", ModelPrice.PerInputCachedInputOutput(0.00000015, 0.000000075, 0.0000006) },

                { "gpt-4-32k", ModelPrice.PerInputOutput(0.00006, 0.00012) },
                { "gpt-4", ModelPrice.PerInputOutput(0.00003, 0.00006) },
            };
        }
        #endregion Prices

        #region Descriptions
        internal static class Descriptions
        {
            // https://platform.openai.com/docs/models
            // Key: ID Keywords
            // Value: Description
            internal static readonly Dictionary<string, string> kDescriptions = new()
            {
                { "curie,babbage,davinci", "GPT base models can understand and generate natural language or code but are not trained with instruction following. These models are made to be replacements for our original GPT-3 base models and use the legacy Completions API. Most customers should use GPT-3.5 or GPT-4." },

                { "omni-moderation", "Moderation models are free models designed to detect harmful content. This model is our most capable moderation model, accepting images as input as well." },
                { "text-moderation", "Moderation models are free models designed to detect harmful content. This is our text only moderation model; we expect omni-moderation-* models to be the best default moving forward." },

                { "text-embedding-3-small", "text-embedding-3-small is our improved, more performant version of our ada embedding model. Embeddings are a numerical representation of text that can be used to measure the relatedness between two pieces of text. Embeddings are useful for search, clustering, recommendations, anomaly detection, and classification tasks." },
                { "text-embedding-3-large", "text-embedding-3-large is our most capable embedding model for both english and non-english tasks. Embeddings are a numerical representation of text that can be used to measure the relatedness between two pieces of text. Embeddings are useful for search, clustering, recommendations, anomaly detection, and classification tasks." },
                { "text-embedding-ada-002", "text-embedding-ada-002 is our improved, more performant version of our ada embedding model. Embeddings are a numerical representation of text that can be used to measure the relatedness between two pieces of text. Embeddings are useful for search, clustering, recommendations, anomaly detection, and classification tasks." },

                { "gpt-4o-search-preview", "GPT-4o Search Preview is a specialized model trained to understand and execute web search queries with the Chat Completions API. In addition to token fees, web search queries have a fee per tool call. Learn more in the pricing page." },
                { "gpt-4o-mini-search-preview", "GPT-4o mini Search Preview is a specialized model trained to understand and execute web search queries with the Chat Completions API. In addition to token fees, web search queries have a fee per tool call. Learn more in the pricing page." },
                { "computer-use-preview", "The computer-use-preview model is a specialized model for the computer use tool. It is trained to understand and execute computer tasks. See the computer use guide for more information. This model is only usable in the Responses API." },

                { "gpt-4o-transcribe", "GPT-4o Transcribe is a speech-to-text model that uses GPT-4o to transcribe audio. It offers improvements to word error rate and better language recognition and accuracy compared to original Whisper models. Use it for more accurate transcripts." },
                { "gpt-4o-mini-transcribe", "GPT-4o mini Transcribe is a speech-to-text model that uses GPT-4o mini to transcribe audio. It offers improvements to word error rate and better language recognition and accuracy compared to original Whisper models. Use it for more accurate transcripts." },
                { "whisper", "Whisper is a general-purpose speech recognition model, trained on a large dataset of diverse audio. You can also use it as a multitask model to perform multilingual speech recognition as well as speech translation and language identification." },

                { "gpt-4o-mini-tts", "GPT-4o mini TTS is a text-to-speech model built on GPT-4o mini, a fast and powerful language model. Use it to convert text to natural sounding spoken text. The maximum number of input tokens is 2000." },
                { "tts-1", "TTS is a model that converts text to natural sounding spoken text. The tts-1 model is optimized for realtime text-to-speech use cases. Use it with the Speech endpoint in the Audio API." },
                { "tts-1-hd", "TTS is a model that converts text to natural sounding spoken text. The tts-1-hd model is optimized for high quality text-to-speech use cases. Use it with the Speech endpoint in the Audio API." },

                { "gpt-image-1", "GPT Image 1 is our new state-of-the-art image generation model. It is a natively multimodal language model that accepts both text and image inputs, and produces image outputs." },
                { "dall-e-3", "DALL·E is an AI system that creates realistic images and art from a natural language description. DALL·E 3 currently supports the ability, given a prompt, to create a new image with a specific size." },
                { "dall-e-2", "DALL·E is an AI system that creates realistic images and art from a natural language description. Older than DALL·E 3, DALL·E 2 offers more control in prompting and more requests at once." },

                { "gpt-4o-realtime", "This is a preview release of the GPT-4o Realtime model, capable of responding to audio and text inputs in realtime over WebRTC or a WebSocket interface." },
                { "gpt-4o-mini-realtime", "This is a preview release of the GPT-4o-mini Realtime model, capable of responding to audio and text inputs in realtime over WebRTC or a WebSocket interface." },

                { "o4-mini", "o4-mini is our latest small o-series model. It's optimized for fast, effective reasoning with exceptionally efficient performance in coding and visual tasks." },
                { "o3-mini", "o3-mini is our newest small reasoning model, providing high intelligence at the same cost and latency targets of o1-mini. o3-mini supports key developer features, like Structured Outputs, function calling, and Batch API." },
                { "o1-mini", "The o1 reasoning model is designed to solve hard problems across domains. o1-mini is a faster and more affordable reasoning model, but we recommend using the newer o3-mini model that features higher intelligence at the same latency and price as o1-mini." },
                { "gpt-4o-mini-audio", "This is a preview release of the smaller GPT-4o Audio mini model. It's designed to input audio or create audio outputs via the REST API." },
                { "gpt-4o-mini", "GPT-4.1 mini provides a balance between intelligence, speed, and cost that makes it an attractive model for many use cases." },
                { "gpt-4o-nano", "GPT-4.1 nano is the fastest, most cost-effective GPT-4.1 model." },

                { "o1-pro", "The o1 series of models are trained with reinforcement learning to think before they answer and perform complex reasoning. The o1-pro model uses more compute to think harder and provide consistently better answers.\n\no1-pro is available in the Responses API only to enable support for multi-turn model interactions before responding to API requests, and other advanced API features in the future." },
                { "o3", "o3 is a well-rounded and powerful model across domains. It sets a new standard for math, science, coding, and visual reasoning tasks. It also excels at technical writing and instruction-following. Use it to think through multi-step problems that involve analysis across text, code, and images." },
                { "o1", "The o1 series of models are trained with reinforcement learning to perform complex reasoning. o1 models think before they answer, producing a long internal chain of thought before responding to the user." },

                { "gpt-4.1", "GPT-4.1 is our flagship model for complex tasks. It is well suited for problem solving across domains." },
                { "gpt-4o", "GPT-4o (“o” for “omni”) is our versatile, high-intelligence flagship model. It accepts both text and image inputs, and produces text outputs (including Structured Outputs). It is the best model for most tasks, and is our most capable model outside of our o-series models." },
                { "gpt-4o-audio", "This is a preview release of the GPT-4o Audio models. These models accept audio inputs and outputs, and can be used in the Chat Completions REST API." },
                { "chatgpt-4o", "ChatGPT-4o points to the GPT-4o snapshot currently used in ChatGPT. GPT-4o is our versatile, high-intelligence flagship model. It accepts both text and image inputs, and produces text outputs. It is the best model for most tasks, and is our most capable model outside of our o-series models.." },

                { "gpt-4-turbo", "GPT-4 Turbo is the next generation of GPT-4, an older high-intelligence GPT model. It was designed to be a cheaper, better version of GPT-4. Today, we recommend using a newer model like GPT-4o." },
                { "gpt-4", "GPT-4 is an older version of a high-intelligence GPT model, usable in Chat Completions." },
                { "gpt-3.5-turbo", "GPT-3.5 Turbo models can understand and generate natural language or code and have been optimized for chat using the Chat Completions API but work well for non-chat tasks as well. As of July 2024, use gpt-4o-mini in place of GPT-3.5 Turbo, as it is cheaper, more capable, multimodal, and just as fast. GPT-3.5 Turbo is still available for use in the API." },
            };
        }
        #endregion Descriptions

        #region Input Modality & Output Modality & Capability

        internal class ModalityCapability
        {
            private static readonly (Modality, Modality, ModelFeature) kGPTBase = (
                   Modality.Text,
                   Modality.Text,
                   ModelFeature.TextGeneration | ModelFeature.FineTuning);

            private static readonly (Modality, Modality, ModelFeature) kGPTLatest = (
                Modality.Text | Modality.Image,
                Modality.Text,
                ModelFeature.TextGeneration | ModelFeature.Streaming | ModelFeature.FunctionCalling | ModelFeature.StructuredOutputs);

            private static readonly (Modality, Modality, ModelFeature) kRealtime = (
                Modality.Text | Modality.Audio,
                Modality.Text | Modality.Audio,
                ModelFeature.Realtime | ModelFeature.FunctionCalling);

            private static readonly (Modality, Modality, ModelFeature) kSearch = (
                Modality.Text,
                Modality.Text,
                ModelFeature.TextGeneration | ModelFeature.Search);

            internal static readonly Dictionary<string, (Modality input, Modality output, ModelFeature caps)> ModelPatterns = new()
            {
                ["babbage"] = kGPTBase,
                ["curie"] = kGPTBase,
                ["davinci"] = kGPTBase,
                ["gpt-3.5"] = kGPTBase,
                ["realtime"] = kRealtime,
                ["search"] = kSearch,
                ["embedding"] = (
                    Modality.Text,
                    Modality.Text,
                    ModelFeature.TextEmbedding
                ),
                ["gpt-4.0"] = kGPTLatest,
                ["gpt-4.1-turbo"] = kGPTLatest,
                ["gpt-4.1-turbo-16k"] = kGPTLatest,
                ["gpt-4.1-turbo-32k"] = kGPTLatest,
                ["gpt-4.1"] = kGPTLatest,
                ["gpt-4o-transcribe"] = (
                    Modality.Text | Modality.Audio,
                    Modality.Text,
                    ModelFeature.SpeechRecognition
                ),
                ["gpt-4o-mini-transcribe"] = (
                    Modality.Text | Modality.Audio,
                    Modality.Text,
                    ModelFeature.SpeechRecognition
                ),
                ["gpt-4o"] = kGPTLatest,
                ["gpt-4-turbo"] = (
                    Modality.Text,
                    Modality.Text,
                    ModelFeature.TextGeneration | ModelFeature.Streaming | ModelFeature.FineTuning | ModelFeature.FunctionCalling
                ),
                ["gpt-4"] = (
                    Modality.Text,
                    Modality.Text,
                    ModelFeature.TextGeneration | ModelFeature.Streaming | ModelFeature.FineTuning
                ),
                ["gpt-3.5"] = (
                    Modality.Text,
                    Modality.Text,
                    ModelFeature.TextGeneration | ModelFeature.FineTuning
                ),
                ["whisper"] = (
                    Modality.Audio,
                    Modality.Text,
                    ModelFeature.SpeechRecognition
                ),
                ["dall-e-2"] = (
                    Modality.Text,
                    Modality.Image,
                    ModelFeature.ImageGeneration | ModelFeature.ImageInpainting
                ),
                ["dall-e-3"] = (
                    Modality.Text,
                    Modality.Image,
                    ModelFeature.ImageGeneration
                ),
                ["gpt-image"] = (
                    Modality.Text | Modality.Image,
                    Modality.Image,
                    ModelFeature.ImageGeneration | ModelFeature.ImageInpainting
                ),
                ["tts-1"] = (
                    Modality.Text,
                    Modality.Audio,
                    ModelFeature.SpeechGeneration
                ),
                ["text-moderation"] = (
                    Modality.Text,
                    Modality.Text,
                    ModelFeature.Moderation
                ),
                ["omni-moderation"] = (
                    Modality.Text | Modality.Image,
                    Modality.Text,
                    ModelFeature.Moderation
                ),
                ["computer-use-preview"] = (
                    Modality.Text | Modality.Image,
                    Modality.Text,
                    ModelFeature.ComputerUse
                ),
                ["o4"] = kGPTLatest,
                ["o3-mini"] = (
                    Modality.Text,
                    Modality.Text,
                    ModelFeature.TextGeneration | ModelFeature.Streaming | ModelFeature.FunctionCalling | ModelFeature.StructuredOutputs
                ),
                ["o3"] = kGPTLatest,
                ["o1-mini"] = (
                    Modality.Text,
                    Modality.Text,
                    ModelFeature.TextGeneration | ModelFeature.Streaming
                ),
                ["o1-pro"] = (
                    Modality.Text | Modality.Image,
                    Modality.Text,
                    ModelFeature.TextGeneration | ModelFeature.FunctionCalling | ModelFeature.StructuredOutputs
                ),
                ["o1"] = kGPTLatest,
            };
        }

        #endregion Input Modality & Output Modality & Capability

        #region Fine-Tuning Utilities

        /// <summary>
        /// Extracts the fine-tuned model name from an ID like "curie:ft-model-name-2023-04-30-07-37-01".
        /// Returns "model-name".
        /// </summary>
        internal static string ResolveFineTunedName(string id)
        {
            // Match ":ft-" and capture everything up to the date
            var match = Regex.Match(id, @":ft-([^:]+?)-\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}");
            var name = match.Success ? match.Groups[1].Value : string.Empty;

            name = name.Replace('-', ' ');
            return name.ToTitleCase();
        }

        /// <summary>
        /// Extracts the timestamp portion from an ID like "curie:ft-model-name-2023-04-30-07-37-01".
        /// Returns "2023-04-30-07-37-01".
        /// </summary>
        internal static string ResolveFineTunedVersion(string id)
        {
            var match = Regex.Match(id, @"(\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2})$");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        internal static string ResolveBaseId(string id)
        {
            return id.Substring(0, id.IndexOf(":ft", System.StringComparison.Ordinal));
        }

        internal static bool IsFineTuned(string id) => id.Contains(":ft");

        #endregion Fine-Tuning Utilities
    }
}