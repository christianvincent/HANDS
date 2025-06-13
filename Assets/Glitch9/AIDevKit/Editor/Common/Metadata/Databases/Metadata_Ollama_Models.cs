namespace Glitch9.AIDevKit.Editor
{
    internal static class Metadata_Ollama_Models
    {
        internal static ModelCatalogueEntry Resolve(ModelCatalogueEntry entry)
        {
            // Missing Properties:
            // ✓ Name
            // ✓ Capability
            // ✘ Version
            // ✘ CreatedAt
            // ✘ Description 
            // ✘ InputModality, OutputModality
            // ✘ InputTokenLimit, OutputTokenLimit 
            // ✓ Provider

            entry.Name = ModelNameResolver.ResolveFromId(entry.Id);
            //entry.Version = ModelMetaUtil.ResolveVersion(entry.Id);
            entry.Capability = ModelFeature.TextGeneration;
            entry.Provider = ModelProviderResolver.Resolve(entry.Id);

            return entry;
        }
    }
}