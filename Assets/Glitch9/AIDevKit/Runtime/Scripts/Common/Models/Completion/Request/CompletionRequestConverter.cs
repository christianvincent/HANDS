using Glitch9.IO.Files;
using Glitch9.IO.Networking.RESTApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Glitch9.AIDevKit
{
    internal class CompletionRequestConverter<TRequest> : JsonConverter<TRequest> where TRequest : CompletionRequestBase
    {
        private readonly Api _api;
        internal CompletionRequestConverter(Api api) => _api = api;

        public override bool CanWrite => true;
        public override void WriteJson(JsonWriter writer, TRequest value, JsonSerializer serializer)
        {
            //AIDevKitDebug.Log("Writing CompletionRequestBase to JSON...");

            JObject obj = new()
            {
                ["model"] = value.Model != null ? JToken.FromObject(value.Model.Id, serializer) : null,
                ["stream"] = value.Stream != null ? JToken.FromObject(value.Stream, serializer) : null,
            };

            bool isOpenAIRequest = _api == Api.OpenAI;
            bool isOpenRouterRequest = _api == Api.OpenRouter;
            bool isOllamaRequest = _api == Api.Ollama;

            if (isOllamaRequest)
            {
                obj["system"] = value.SystemInstruction != null ? JToken.FromObject(value.SystemInstruction, serializer) : null;
                obj["keep_alive"] = value.KeepAlive != null ? JToken.FromObject(value.KeepAlive, serializer) : null;
                obj["options"] = value.ModelOptions != null ? JToken.FromObject(value.ModelOptions, serializer) : null; // 통째로 serialize 
            }
            else
            {
                obj["n"] = value.N != null ? JToken.FromObject(value.N, serializer) : null;

                if (value.ModelOptions != null)
                {
                    obj["max_tokens"] = value.ModelOptions.MaxTokens != null ? JToken.FromObject(value.ModelOptions.MaxTokens, serializer) : null;

                    obj["temperature"] = value.ModelOptions.Temperature != null ? JToken.FromObject(value.ModelOptions.Temperature, serializer) : null;
                    obj["seed"] = value.ModelOptions.Seed != null ? JToken.FromObject(value.ModelOptions.Seed, serializer) : null;
                    obj["stop"] = value.ModelOptions.Stop != null ? JToken.FromObject(value.ModelOptions.Stop, serializer) : null;

                    obj["frequency_penalty"] = value.ModelOptions.FrequencyPenalty != null ? JToken.FromObject(value.ModelOptions.FrequencyPenalty, serializer) : null;
                    obj["presence_penalty"] = value.ModelOptions.PresencePenalty != null ? JToken.FromObject(value.ModelOptions.PresencePenalty, serializer) : null;

                    obj["logit_bias"] = value.ModelOptions.LogitBias != null ? JToken.FromObject(value.ModelOptions.LogitBias, serializer) : null;
                    obj["logprobs"] = value.ModelOptions.Logprobs != null ? JToken.FromObject(value.ModelOptions.Logprobs, serializer) : null;
                    obj["top_logprobs"] = value.ModelOptions.TopLogprobs != null ? JToken.FromObject(value.ModelOptions.TopLogprobs, serializer) : null;

                    obj["top_p"] = value.ModelOptions.TopP != null ? JToken.FromObject(value.ModelOptions.TopP, serializer) : null;

                    if (isOpenRouterRequest)
                    {
                        // OpenRouter-only options: top_k, top_a, min_p, repetition_penalty                    
                        obj["top_k"] = value.ModelOptions.TopK != null ? JToken.FromObject(value.ModelOptions.TopK, serializer) : null;
                        obj["top_a"] = value.ModelOptions.TopA != null ? JToken.FromObject(value.ModelOptions.TopA, serializer) : null;
                        obj["min_p"] = value.ModelOptions.MinP != null ? JToken.FromObject(value.ModelOptions.MinP, serializer) : null;
                        obj["repetition_penalty"] = value.ModelOptions.RepeatPenalty != null ? JToken.FromObject(value.ModelOptions.RepeatPenalty, serializer) : null;
                    }
                }

                if (isOpenRouterRequest)
                {
                    obj["metadata"] = value.Metadata != null ? JToken.FromObject(value.Metadata, serializer) : null;
                    obj["user"] = value.User != null ? JToken.FromObject(value.User, serializer) : null;
                    obj["usage"] = value.StreamOptions != null ? JToken.FromObject(value.StreamOptions.IncludeUsage, serializer) : null;
                    obj["models"] = value.Models != null ? JToken.FromObject(value.Models, serializer) : null;
                    obj["transforms"] = value.Transforms != null ? JToken.FromObject(value.Transforms, serializer) : null;
                    obj["prompt"] = value.Prompt != null ? JToken.FromObject(value.Prompt, serializer) : null;
                }
            }

            if (value is CompletionRequest completion)
            {
                if (isOllamaRequest)
                {
                    obj["prompt"] = value.Prompt != null ? JToken.FromObject(value.Prompt, serializer) : null;
                    obj["suffix"] = completion.Suffix != null ? JToken.FromObject(completion.Suffix, serializer) : null;
                    obj["template"] = completion.Template != null ? JToken.FromObject(completion.Template, serializer) : null;
                    obj["raw"] = completion.Raw != null ? JToken.FromObject(completion.Raw, serializer) : null;
                    obj["context"] = completion.Context != null ? JToken.FromObject(completion.Context, serializer) : null;
                }
            }
            else if (value is ChatCompletionRequest chat)
            {
                if (!string.IsNullOrEmpty(value.Prompt))
                {
                    bool addPromptAsMessage = !isOllamaRequest;

                    if (!addPromptAsMessage && isOpenAIRequest)
                    {
                        bool isLegacy = value.Model != null && value.Model.IsLegacy;
                        if (isLegacy) obj["prompt"] = value.Prompt != null ? JToken.FromObject(value.Prompt, serializer) : null;
                    }

                    if (addPromptAsMessage)
                    {
                        UserMessage m = new(value.Prompt);
                        chat.Messages.Add(m);
                    }
                }

                if (!chat.AttachedFiles.IsNullOrEmpty())
                {
                    if (isOllamaRequest)
                    {
                        List<string> base64Images = new();

                        foreach (IFile file in chat.AttachedFiles)
                        {
                            if (file is File<Texture2D> image)
                            {
                                string base64 = image.EncodeToBase64();
                                if (!string.IsNullOrEmpty(base64)) base64Images.Add(base64);
                            }
                        }

                        if (!base64Images.IsNullOrEmpty()) obj["images"] = JToken.FromObject(base64Images, serializer);
                    }
                    else
                    {
                        List<ContentPart> parts = new();

                        foreach (IFile f in chat.AttachedFiles)
                        {
                            if (f is File<Texture2D> image)
                            {
                                string base64 = image.EncodeToBase64();
                                if (string.IsNullOrEmpty(base64)) continue;

                                parts.Add(ImageContentPart.FromBase64(base64));
                            }
                            else if (f is File<AudioClip> audio)
                            {
                                string base64 = audio.EncodeToBase64();
                                if (string.IsNullOrEmpty(base64)) continue;

                                parts.Add(AudioContentPart.FromBase64(base64));
                            }
                            else if (f is RawFile file)
                            {
                                string base64 = file.EncodeToBase64();
                                if (string.IsNullOrEmpty(base64)) continue;

                                string fileName = file.Name;
                                parts.Add(FileContentPart.FromBase64(base64, fileName));
                            }

                            if (parts.Count > 0)
                            {
                                ChatMessage m = chat.Messages.LastOrDefault();

                                if (m == null || m.Role != ChatRole.User)
                                {
                                    m = new UserMessage(null);
                                    chat.Messages.Add(m);
                                }

                                m.Content.AddPartRange(parts);
                                chat.Messages.ReplaceLastMessage(m);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(chat.StartingMessage)) chat.Messages.SetStartingMessage(chat.StartingMessage);  // Must do this after setting the messages 
                if (!string.IsNullOrEmpty(chat.Summary)) chat.Messages.SetSummary(chat.Summary); // Must do this after setting the messages

                if (!isOllamaRequest)
                {
                    obj["tool_choice"] = chat.ToolChoice != null ? JToken.FromObject(chat.ToolChoice, serializer) : null;
                    obj["response_format"] = chat.ResponseFormat != null ? JToken.FromObject(chat.ResponseFormat, serializer) : null;
                    obj["audio"] = chat.Audio != null ? JToken.FromObject(chat.Audio, serializer) : null;
                    obj["modalities"] = chat.Modalities != null ? JToken.FromObject(chat.Modalities, serializer) : null;
                    obj["web_search_options"] = chat.WebSearchOptions != null ? JToken.FromObject(chat.WebSearchOptions, serializer) : null;

                    if (!string.IsNullOrEmpty(chat.SystemInstruction)) chat.Messages.SetSystemInstruction(chat.SystemInstruction);

                    if (isOpenAIRequest) // OpenAI는 ChatCompletionRequest밖에 없다.
                    {
                        obj["stream_options"] = value.StreamOptions != null ? JToken.FromObject(value.StreamOptions, serializer) : null;

                        if (value.ReasoningOptions != null && value.ReasoningOptions.Effort != null && value.ReasoningOptions.Effort != ReasoningEffort.Medium)
                        {
                            obj["reasoning_effort"] = JToken.FromObject(value.ReasoningOptions.Effort, serializer);
                        }

                        obj["service_tier"] = chat.ServiceTier != null ? JToken.FromObject(chat.ServiceTier, serializer) : null;

                        if (value.ModelOptions != null)
                        {
                            // OpenAI-only options: logprobs
                            obj["logprobs"] = value.ModelOptions.Logprobs != null ? JToken.FromObject(value.ModelOptions.Logprobs, serializer) : null;
                        }
                    }
                }

                obj["messages"] = chat.Messages != null ? JToken.FromObject(chat.Messages, serializer) : null;
                obj["tools"] = chat.Tools != null ? JToken.FromObject(chat.Tools, serializer) : null;
            }

            // Check "messages" and "prompt" to see if they both exist
            if (obj.ContainsKey("messages") && obj.ContainsKey("prompt"))
            {
                // If both exist, remove "prompt"
                obj.Remove("prompt");
            }

            obj.RemoveNulls(); // null 제거 
            obj.WriteTo(writer);
        }

        public override bool CanRead => false;
        public override TRequest ReadJson(JsonReader reader, Type objectType, TRequest existingValue, bool hasExistingValue, JsonSerializer serializer)
            => throw new NotImplementedException();
    }
}
