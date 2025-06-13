using System;

namespace Glitch9.AIDevKit
{
    internal static class InternalExtensions
    {
        internal static bool IsDallE2(this Model model) => IsModelId(model, "dall-e-2");
        internal static bool IsDallE3(this Model model) => IsModelId(model, "dall-e-3");
        internal static bool IsGptImage1(this Model model) => IsModelId(model, "gpt-image-1");
        internal static bool IsGemini(this Model model) => model != null && model.Family == ModelFamily.Gemini;
        internal static bool IsImagen(this Model model) => model != null && model.Family == ModelFamily.Imagen;
        internal static bool IsLLM(this Model model) => model != null && model.Capability.HasFlag(ModelFeature.TextGeneration);

        private static bool IsModelId(this Model model, string id) => model != null && model.Id == id;

        internal static string SafeGetId(this Model model)
        {
            if (model == null) return string.Empty;
            return model.Id ?? string.Empty;
        }

        internal static string SafeGetApiName(this Model model)
        {
            if (model == null) return string.Empty;
            return model.Api.GetInspectorName();
        }

        internal static string SafeGetName(this Model model, string fallback = "Unknown Model")
        {
            if (model == null) return fallback;
            return model.Name ?? model.Id ?? fallback;
        }

        internal static string SafeGetId(this Voice voice)
        {
            if (voice == null) return string.Empty;
            return voice.Id ?? string.Empty;
        }

        internal static string SafeGetApiName(this Voice voice)
        {
            if (voice == null) return string.Empty;
            return voice.Api.GetInspectorName();
        }

        internal static string SafeGetName(this Voice voice, string fallback = "Unknown Voice")
        {
            if (voice == null) return fallback;
            return voice.Name ?? voice.Id ?? fallback;
        }

        internal static void ProcessText(this ChatCompletion c, Func<string, string> textProcessor)
        {
            if (c == null || textProcessor == null) return;

            if (c.Choices.IsNullOrEmpty()) return;

            string text = c.ToString();

            if (string.IsNullOrEmpty(text)) return;

            string newText = textProcessor(text);

            if (newText == text) return;

            foreach (var choice in c.Choices)
            {
                if (choice?.Message is ChatMessage message)
                {
                    message.ReplaceText(newText);
                }
                else if (choice?.Delta is ChatDelta delta)
                {
                    delta.Content = newText;
                }
                else
                {
                    choice.Text = newText;
                }
            }
        }

        internal static void ReplaceText(this ChatMessage m, string newText)
        {
            if (m == null || m.Content == null || newText == null) return;

            m.Content.ReplaceText(newText);
        }
    }
}