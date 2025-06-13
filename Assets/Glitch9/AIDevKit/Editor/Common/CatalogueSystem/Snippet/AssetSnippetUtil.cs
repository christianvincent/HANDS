using System.Collections.Generic;

namespace Glitch9.AIDevKit.Editor
{
    internal static class AssetSnippetUtil
    {
        // For all the following constants, {0} is the provider name (e.g., ServiceProvider.OpenAI.ToString())
        private const string kNamespaceFormat = "Glitch9.AIDevKit.{0}";
        private const string kModelClassNameFormat = "{0}Model";
        private const string kVoiceClassNameFormat = "{0}Voice";

        internal static string ResolveNamespace(Api api) => string.Format(kNamespaceFormat, api.ToString());
        internal static string ResolveModelClassName(Api api) => string.Format(kModelClassNameFormat, api.ToString());
        internal static string ResolveVoiceClassName(Api api) => string.Format(kVoiceClassNameFormat, api.ToString());

        private static readonly Dictionary<string, string> _modelSnippetParsingRules = new()
        {
            { "gpt", "GPT" },
            { "chatgpt", "ChatGPT" },
            { "tts", "TTS" },
            { "dalle", "DallE" },
            { "hd", "HD" },
            { "ft", "FT" },
        };

        internal static string ResolveModelPropertyName(string id)
        {
            id = ModelMetaUtil.RemoveSlashPrefix(id);

            if (id.Contains("dall-e")) id = id.Replace("dall-e", "dalle");

            id = id
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace(".", "_")
                .Replace(":", "_")
                .Replace("(", "")
                .Replace(")", "")
                .Trim();

            string[] parts = id.Split('_');
            List<string> parsedParts = new();

            foreach (string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;
                string parsedPart = part.ToLowerInvariant();

                if (_modelSnippetParsingRules.ContainsKey(parsedPart))
                {
                    parsedPart = _modelSnippetParsingRules[parsedPart];
                }
                else
                {
                    parsedPart = parsedPart.CapFirstChar();
                }

                parsedParts.Add(parsedPart);
            }

            if (ModelMetaUtil.IsOModel(parsedParts[0]))
            {
                parsedParts[0] = parsedParts[0].UncapFirstChar();
            }
            else
            {
                // if second part starts with a number, add it to the first part
                if (parsedParts.Count > 1 && char.IsDigit(parsedParts[1][0]))
                {
                    parsedParts[0] += parsedParts[1];
                    parsedParts.RemoveAt(1);
                }
            }

            return string.Join("_", parsedParts);
        }

        internal static string ResolveVoicePropertyName(Voice profile)
        {
            if (profile.Api == Api.ElevenLabs)
            {
                return profile.Name.Trim().ToPascalCase();
            }
            return profile.Name.Trim().ToPascalCase();
        }
    }
}