using Glitch9.IO.Files;
using Glitch9.IO.Networking.RESTApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glitch9.AIDevKit
{
    #region Class Design
    // OpenAI and Gemini can have more than one 'content', how am I going to handle that?
    // The bottom line is that one message can have all the following:
    //
    // 1. Sending & Receiving Texts (text)
    //  - You can send a chat message as a string (text).
    //  - You can receive a chat message as a string (text).
    //
    // 2. Sending Images
    //  - You can send an image as a base64-encoded string (OpenAI and Gemini both support this).
    //  - You can send an image as a URL (OpenAI only; Gemini requires inline_data).
    //
    // 3. Receiving Images
    //  - You can receive an image as a base64-encoded string.
    //  - You can receive an image as a URL.
    //  - You can receive an image as a file ID in the following conditions:
    //    (a) When using the Assistants API's code_interpreter tool to generate a file
    //    (b) When using the Assistants API's file_search tool to locate a document
    //    (c) When using the Assistants API's retrieval tool to reference an uploaded file
    //  (Note: This does not mean the model *generates* new images like Stable Diffusion; images are provided as part of tool outputs.)
    //
    // 4. Sending & Receiving Files (Only supported via the OpenAI Assistants API)
    //  - You can send a file as a file ID.
    //  - You can receive a file as a file ID. 

    // Can there be more than one Text inside Content?
    //
    // Answer
    //  Yes, there can be multiple Text parts inside Content. (Especially when using vision models)
    //
    // Why?
    //  According to the official OpenAI spec (gpt-4o, gpt-4-vision-preview),
    //  Content can be sent as an array, mixing text parts and image parts.
    //
    // It's completely allowed to have multiple "text" types inside the array.
    //
    // Example
    // This is a completely valid request:
    // {
    //     "role": "user",
    //     "content": [
    //         { "type": "text", "text": "First question." },
    //         { "type": "image", "image_url": { "url": "https://example.com/image.png" } },
    //         { "type": "text", "text": "Second question. Please further analyze the image." }
    //     ]
    // }
    //
    // In this case:
    // - Text → Image → Text sequence is composed inside Content array.
    // - There are two separate "text" parts.
    //
    // This is perfectly valid.
    //
    // Possibility summary:
    // - Can there be multiple Text parts in Content? Yes
    // - Can there be multiple Image parts in Content? Yes
    // - Can Text and Image parts be mixed? Yes
    // - Is the order preserved? Yes (processed in array order)
    //
    // Note:
    // - This is only possible when Content is an array.
    // - If you send Content as a plain string, obviously only one Text will exist.
    // - So when designing your model, **always assume Content will be a list**.
    // - (Most vision and multimodal models already enforce Content as a list.)
    #endregion Class Design

    [JsonObject]
    public class SystemMessage : ChatMessage
    {
        public static implicit operator string(SystemMessage message) => new(message.ToString());
        [JsonProperty("role")] public override ChatRole Role => ChatRole.System;
        public SystemMessage() { }
        public SystemMessage(Content content) : base(content) { }
    }

    [JsonObject]
    public class UserMessage : ChatMessage
    {
        public static implicit operator string(UserMessage message) => new(message.ToString());
        [JsonProperty("role")] public override ChatRole Role => ChatRole.User;

        // --- Local variables ------------------------------------------------------------ 
        [JsonIgnore] public List<IFile> AttachedFiles { get; set; }
        [JsonIgnore] public List<SafetyRating> Moderations { get; set; }

        public UserMessage() { }
        public UserMessage(Content content) : base(content) { }

        public void AddAttachment<T>(T file) where T : IFile
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            AttachedFiles ??= new();
            AttachedFiles.Add(file);
        }

        public void AddAttachments<T>(IList<T> files) where T : IFile
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            AttachedFiles ??= new();
            foreach (T file in files)
            {
                if (file == null) continue;
                AttachedFiles.Add(file);
            }
        }

        public T GetFirstFile<T>() where T : class, IFile
        {
            if (AttachedFiles == null) return null;
            foreach (IFile file in AttachedFiles)
            {
                if (file is T t) return t;
            }
            return null;
        }
    }

    [JsonObject]
    public class ResponseMessage : ChatMessage
    {
        public static implicit operator string(ResponseMessage message) => new(message.ToString());

        [JsonProperty("role")] public override ChatRole Role => ChatRole.Assistant;

        /// <summary>
        /// Optional. A list of tool calls (functions) the model wants to use.
        /// </summary>
        [JsonProperty("tool_calls")] public ToolCall[] Tools { get; set; }

        // --- Local variables ------------------------------------------------------------ 
        [JsonIgnore] public Usage Usage { get; set; }

        public ResponseMessage() { }
        public ResponseMessage(Content content) : base(content) { }

        internal static ResponseMessage FromChatCompletion(ChatCompletion chat)
        {
            if (chat == null) throw new ArgumentNullException(nameof(chat));
            return new ResponseMessage(chat.FirstContent()) { Usage = chat.Usage, Tools = chat.GetToolCalls() };
        }

        internal static ResponseMessage ErrorMessage(string errorMessage)
        {
            const string kDefaultError = "I'm sorry, There was an error processing your request.";
            string content = string.IsNullOrEmpty(errorMessage) ? kDefaultError : errorMessage;
            return new ResponseMessage(content);
        }
    }

    [JsonObject]
    public class ToolMessage : ResponseMessage
    {
        public static implicit operator string(ToolMessage message) => new(message.ToString());
        [JsonProperty("role")] public override ChatRole Role => ChatRole.Tool;
        public ToolMessage() { }
        public ToolMessage(Content content) : base(content) { }
    }

    [JsonObject]
    public class ImageMessage : ResponseMessage
    {
        public static implicit operator string(ImageMessage message) => new(message.ToString());

        /// <summary>
        /// The image content.
        /// </summary>
        [JsonProperty("image")] public GeneratedImage Image { get; set; }

        public ImageMessage() { }
        public ImageMessage(GeneratedImage image) : base()
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
        }
    }

    [JsonObject]
    public abstract class ChatMessage
    {
        public static implicit operator string(ChatMessage message) => new(message.ToString());
        public static implicit operator ChatMessage(string message) => new UserMessage(message);

        /// <summary>
        /// The role of the messages author.
        /// </summary>
        [JsonProperty("role")] public abstract ChatRole Role { get; }

        /// <summary>
        /// The contents of the user message.
        /// </summary>
        [JsonProperty("content")] public Content Content { get; set; }

        /// <summary> 
        /// Optional. An optional name for the participant.
        /// Provides the model information to differentiate between participants of the same role.
        /// </summary>
        [JsonProperty("name")] public string Name { get; set; }

        // --- Local variables ------------------------------------------------------------  
        [JsonIgnore] public UnixTime Timestamp { get; internal set; } = UnixTime.Now;
        public override string ToString() => Content?.ToString() ?? string.Empty;

        public ChatMessage() { }
        public ChatMessage(Content content = null) => Content = content;

        public static ChatMessage Create(ChatRole role, Content content = null)
        {
            return role switch
            {
                ChatRole.System => new SystemMessage(content),
                ChatRole.User => new UserMessage(content),
                ChatRole.Assistant => new ResponseMessage(content),
                ChatRole.Tool => new ToolMessage(content),
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
            };
        }
    }

    #region JsonConverter
    internal class ChatMessageConverter : JsonConverter<ChatMessage>
    {
        private readonly Api _provider;
        internal ChatMessageConverter(Api provider) => _provider = provider;

        public override ChatMessage ReadJson(JsonReader reader, Type objectType, ChatMessage existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            ChatRole role = obj["role"]?.ToObject<ChatRole>(serializer) ?? ChatRole.User;
            string name = obj["name"]?.ToString();

            if (_provider == Api.None)
            {
                switch (role)
                {
                    case ChatRole.System:
                        return new SystemMessage()
                        {
                            Name = name,
                            Content = obj["content"]?.ToObject<Content>(serializer),
                            Timestamp = obj["timestamp"]?.ToObject<UnixTime>(serializer) ?? UnixTime.Now,
                        };
                    case ChatRole.User:
                        return new UserMessage()
                        {
                            Name = name,
                            Content = obj["content"]?.ToObject<Content>(serializer),
                            AttachedFiles = obj["attachments"]?.ToObject<List<IFile>>(serializer),
                            Moderations = obj["moderations"]?.ToObject<List<SafetyRating>>(serializer),
                            Timestamp = obj["timestamp"]?.ToObject<UnixTime>(serializer) ?? UnixTime.Now,
                        };
                    case ChatRole.Assistant:
                        return new ResponseMessage(obj["content"]?.ToString())
                        {
                            Name = name,
                            Content = obj["content"]?.ToObject<Content>(serializer),
                            Tools = obj["tools"]?.ToObject<ToolCall[]>(serializer),
                            Usage = obj["usage"]?.ToObject<Usage>(serializer),
                            Timestamp = obj["timestamp"]?.ToObject<UnixTime>(serializer) ?? UnixTime.Now,
                        };
                    case ChatRole.Tool:
                        return new ToolMessage(obj["content"]?.ToString())
                        {
                            Name = name,
                            Content = obj["content"]?.ToObject<Content>(serializer),
                            Tools = obj["tools"]?.ToObject<ToolCall[]>(serializer),
                            Usage = obj["usage"]?.ToObject<Usage>(serializer),
                            Timestamp = obj["timestamp"]?.ToObject<UnixTime>(serializer) ?? UnixTime.Now,
                        };
                    default:
                        Debug.LogWarning($"[ChatMessageConverter] Unknown role: {role}");
                        return null;
                }
            }
            else
            {
                JToken contentToken = obj["content"];

                Content content = null;

                if (contentToken != null && contentToken.Type != JTokenType.Null)
                {
                    if (contentToken.Type == JTokenType.Object || contentToken.Type == JTokenType.Array)
                    {
                        try
                        {
                            content = contentToken.ToObject<Content>(serializer);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[ChatMessageConverter] Failed to deserialize ChatContent: {ex.Message}");
                        }
                    }
                    if (contentToken.Type == JTokenType.String)
                    {
                        content = new Content(contentToken.ToString());
                    }
                    else
                    {
                        Debug.LogWarning($"[ChatMessageConverter] Unexpected content token type: {contentToken.Type}");
                    }
                }

                ToolCall[] toolCalls = null;
                bool hasToolCalls = role == ChatRole.Tool || role == ChatRole.Assistant;

                if (hasToolCalls)
                {
                    if (obj["tools"] != null && obj["tools"].Type != JTokenType.Null)
                    {
                        toolCalls = obj["tools"].ToObject<ToolCall[]>(serializer);
                    }
                    else
                    {
                        toolCalls = null;
                    }
                }

                // return new ChatMessage
                // {
                //     Role = obj["role"]?.ToObject<ChatRole>(serializer) ?? ChatRole.User,
                //     //Content = obj["content"]?.ToObject<ChatContent>(serializer),
                //     Content = content,
                //     Name = obj["name"]?.ToString(),
                //     //Tools = obj["tool_calls"]?.ToObject<ToolCall[]>(serializer),
                //     Tools = toolCalls,
                // };

                switch (role)
                {
                    case ChatRole.System:
                        return new SystemMessage()
                        {
                            Name = name,
                            Content = content,
                        };
                    case ChatRole.User:
                        return new UserMessage()
                        {
                            Name = name,
                            Content = content,
                        };
                    case ChatRole.Assistant:
                        return new ResponseMessage()
                        {
                            Name = name,
                            Content = content,
                            Tools = toolCalls,
                        };
                    case ChatRole.Tool:
                        return new ToolMessage()
                        {
                            Name = name,
                            Content = content,
                            Tools = toolCalls,
                        };
                    default:
                        Debug.LogWarning($"[ChatMessageConverter] Unknown role: {role}");
                        return null;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, ChatMessage value, JsonSerializer serializer)
        {
            //AIDevKitDebug.Log("ChatMessageConverter: Writing ChatMessage to JSON");

            JObject obj;

            if (_provider == Api.None)
            {
                obj = new()
                {
                    ["role"] = JToken.FromObject(value.Role, serializer),
                    ["content"] = value.Content != null ? JToken.FromObject(value.Content, serializer) : null,
                    ["name"] = value.Name != null ? JToken.FromObject(value.Name, serializer) : null,
                    ["timestamp"] = value.Timestamp != null ? JToken.FromObject(value.Timestamp, serializer) : null,
                    // ["attachments"] = value.AttachedFiles != null ? JArray.FromObject(value.AttachedFiles, serializer) : null,
                    // ["moderations"] = value.Moderations != null ? JArray.FromObject(value.Moderations, serializer) : null,
                    // ["usage"] = value.Usage != null ? JToken.FromObject(value.Usage, serializer) : null,
                    // ["tools"] = value.Tools != null ? JArray.FromObject(value.Tools, serializer) : null,
                };

                // log role and the class type
                // AIDevKitDebug.Log($"[ChatMessageConverter] Writing JSON for role: {value.Role}, class: {value.GetType().Name}");

                if (value is UserMessage userMessage)
                {
                    obj["attachments"] = userMessage.AttachedFiles != null ? JArray.FromObject(userMessage.AttachedFiles, serializer) : null;
                    obj["moderations"] = userMessage.Moderations != null ? JArray.FromObject(userMessage.Moderations, serializer) : null;
                }
                else if (value is ResponseMessage assistantMessage)
                {
                    obj["tools"] = assistantMessage.Tools != null ? JArray.FromObject(assistantMessage.Tools, serializer) : null;
                    obj["usage"] = assistantMessage.Usage != null ? JToken.FromObject(assistantMessage.Usage, serializer) : null;
                }
                else if (value is ToolMessage toolMessage)
                {
                    obj["tools"] = toolMessage.Tools != null ? JArray.FromObject(toolMessage.Tools, serializer) : null;
                    obj["usage"] = toolMessage.Usage != null ? JToken.FromObject(toolMessage.Usage, serializer) : null;
                }
            }
            else
            {
                obj = new()
                {
                    ["role"] = JToken.FromObject(value.Role, serializer),
                    ["content"] = value.Content != null ? JToken.FromObject(value.Content, serializer) : "empty", // Unity crashes if this is null
                    ["name"] = value.Name != null ? JToken.FromObject(value.Name, serializer) : null,
                };
            }

            obj.RemoveNulls(); // null 제거 
            obj.WriteTo(writer);
        }
    }
    #endregion JsonConverter

    #region Extensions
    // Extensions for List<ChatMessage> to manage chat messages
    internal static class ChatMessageExtensions
    {
        internal static void SetSystemInstruction(this List<ChatMessage> list, string systemInstruction)
        {
            if (string.IsNullOrEmpty(systemInstruction)) return;
            list ??= new();
            list.Insert(0, new SystemMessage(systemInstruction));
        }

        internal static void SetStartingMessage(this List<ChatMessage> list, string startingMessage)
        {
            if (string.IsNullOrWhiteSpace(startingMessage)) return;
            list ??= new();

            if (list.Count == 0)
            {
                list.Add(new ResponseMessage(startingMessage));
                return;
            }

            int targetIndex = 0;

            while (targetIndex < list.Count && list[targetIndex].Role == ChatRole.System)
            {
                targetIndex++;
            }

            bool insert = targetIndex == list.Count || list[targetIndex].Role != ChatRole.Assistant;

            if (insert)
            {
                list.Insert(targetIndex, new ResponseMessage(startingMessage));
            }
            else
            {
                list[targetIndex] = new ResponseMessage(startingMessage);
            }
        }

        internal static void SetSummary(this List<ChatMessage> list, string summary)
        {
            if (string.IsNullOrWhiteSpace(summary)) return;
            list ??= new();

            string parsedSummary = $"Here is a summary of the conversation so far:\n{summary}";

            if (list.Count == 0)
            {
                list.Add(new SystemMessage(parsedSummary));
                return;
            }

            int targetIndex = 0;
            if (list[targetIndex].Role == ChatRole.System) targetIndex = 1;

            if (list[targetIndex].Role == ChatRole.System)
            {
                list[targetIndex] = new SystemMessage(parsedSummary);
            }
            else
            {
                list.Insert(targetIndex, new SystemMessage(parsedSummary));
            }
        }

        internal static void ReplaceLastMessage(this List<ChatMessage> list, ChatMessage message)
        {
            if (list == null || list.Count == 0 || message == null) return;
            list[^1] = message;
        }
    }
    #endregion Extensions
}