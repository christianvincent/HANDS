
using System;
using Newtonsoft.Json;

namespace Glitch9.AIDevKit
{
    public class ChatCompletion : AIResponse, IGeneratedResult
    {
        public static implicit operator Content(ChatCompletion chat) => chat?.FirstContent();
        public static implicit operator string(ChatCompletion chat) => chat?.ToString() ?? string.Empty;

        /// <summary>
        /// A list of chat completion choices. Can be more than one if n is greater than 1.
        /// <para>
        /// If this is 'ChatCompletionChunk' which is a streamed response,
        /// This can also be empty for the last chunk if you set stream_options: {"include_usage": true}.
        /// </para>
        /// </summary>
        [JsonProperty("choices")] public ChatChoice[] Choices { get; set; }

        /// <summary>
        /// This fingerprint represents the backend configuration that the model runs with.
        /// Can be used in conjunction with the seed request parameter to understand when backend changes have been made that might impact determinism.
        /// </summary>
        [JsonProperty("system_fingerprint")] public string SystemFingerprint { get; set; }

        public int Count => Choices.Length;
        public bool IsEmpty => Choices.IsNullOrEmpty();

        public override string ToString() => Choices.GetFirstMessageText();
        public string[] ToStringArray() => Choices.GetStringArray();
        public ToolCall[] GetToolCalls() => Choices.GetToolCalls();

        public ChatDelta FirstDelta() => Choices.GetFirstDelta();
        public ChatDelta[] GetDeltaChunks() => Choices.GetDeltaChunks();

        public Content FirstContent() => Choices.GetFirstContent();
        public Content[] GetContents() => Choices.GetContents();

        public ResponseMessage FirstResponseMessage() => Choices.GetFirstResponseMessage();
        public ResponseMessage[] GetResponseMessages() => Choices.GetResponseMessages();

    }
}
