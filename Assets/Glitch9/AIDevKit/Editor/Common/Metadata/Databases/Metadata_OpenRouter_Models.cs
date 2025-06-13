namespace Glitch9.AIDevKit.Editor
{
    internal static class Metadata_OpenRouter_Models
    {
        internal static ModelCatalogueEntry Resolve(ModelCatalogueEntry entry)
        {
            // Missing Properties:
            // ✓ Capability
            // ✘ Version
            // ✓ Provider

            //entry.Name = entry.Id;

            if (!string.IsNullOrWhiteSpace(entry.Name) && entry.Name.Contains("(free)"))
            {
                entry.SetPrices(ModelPrice.Free());
            }

            entry.Provider = ResolveProvider(entry.Id);

            bool capabilityFound = false;

            if (entry.Provider == Api.OpenAI.ToString())
            {
                if (Metadata_OpenAI_Models.TryGetModalityCapability(entry.Id, out (Modality, Modality, ModelFeature cap) result))
                {
                    entry.Capability = result.cap;
                    capabilityFound = true;
                }
            }

            if (!capabilityFound)
            {
                if (Metadata_ModelFeatures.Map.TryGetValue(entry.Id, out ModelFeature capability))
                {
                    entry.Capability = capability;
                }
                else
                {
                    entry.Capability = ModelFeature.TextGeneration;
                }
            }

            return entry;
        }

        internal static double ResolveCost(string costAsString)
        {
            if (string.IsNullOrEmpty(costAsString)) return 0.0;
            return double.TryParse(costAsString, out double result) ? result : 0.0;
        }

        private static string ResolveProvider(string id)
        {
            if (id.Contains('/'))
            {
                string[] parts = id.Split('/');
                id = parts[0].CapFirstChars('-').Trim();

                if (id.EndsWith("ai") || id.EndsWith("-Ai"))
                {
                    // capitalize ai => AI
                    id = id[..^2] + "AI";
                }

                // if id starts with "Ai" and the third char is not a letter, capitalize AI
                if (id.StartsWith("Ai") && id.Length > 2 && !char.IsLetter(id[2]))
                {
                    id = "AI" + id[2..];
                }

                if (id.Contains("Deepseek"))
                {
                    id = id.Replace("Deepseek", "DeepSeek");
                }
            }

            return id;
        }
    }
}