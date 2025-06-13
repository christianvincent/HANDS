
namespace Glitch9.AIDevKit
{
    public class ChatCompletionChunk
    {
        internal ChatCompletion delta;
        internal bool isDone;
        internal bool isError;
        internal string errorMessage;
        internal Usage Usage => delta?.Usage;

        internal static ChatCompletionChunk Delta(ChatCompletion delta)
        {
            return new ChatCompletionChunk
            {
                delta = delta,
            };
        }

        internal static ChatCompletionChunk Error(string error)
        {
            return new ChatCompletionChunk
            {
                isError = true,
                errorMessage = error ?? "Unknown error",
            };
        }

        internal static ChatCompletionChunk Done()
        {
            return new ChatCompletionChunk
            {
                isDone = true,
            };
        }
    }
}