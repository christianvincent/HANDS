using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Glitch9.AIDevKit.Editor
{
    [JsonObject]
    public class ModelCatalogueEntry : IModelData, ICatalogueAsset
    {
        // --- Core Properties ---
        [JsonProperty] public Api Api { get; internal set; }
        [JsonProperty] public string Id { get; internal set; }
        [JsonProperty] public string Name { get; internal set; }
        [JsonProperty] public string Family { get; internal set; }
        [JsonProperty] public string Version { get; internal set; }
        [JsonProperty] public string FamilyVersion { get; internal set; }
        [JsonProperty] public string Description { get; internal set; }
        [JsonProperty] public string OwnedBy { get; internal set; }
        [JsonProperty] public UnixTime? CreatedAt { get; internal set; }
        [JsonProperty] public bool IsLegacy { get; internal set; } = false;
        [JsonProperty] public bool IsDeprecated { get; set; } = false;


        // --- Token Limit Properties ---
        [JsonProperty] public int? InputTokenLimit { get; internal set; }
        [JsonProperty] public int? OutputTokenLimit { get; internal set; }


        // --- Fine-tuning Related Properties ---  
        [JsonProperty] public string BaseId { get; internal set; }
        [JsonProperty] public bool IsFineTuned { get; internal set; }

        // --- Resolved Properties (not in IModelData) ---
        [JsonProperty] public string Provider { get; internal set; }
        [JsonProperty] public ModelFeature Capability { get; internal set; }
        [JsonProperty] public Modality InputModality { get; internal set; }
        [JsonProperty] public Modality OutputModality { get; internal set; }


        // Pricing TEST << THIS IS TEST ONLY, NOT FINAL >>
        [JsonProperty] public Dictionary<UsageType, double> Pricing { get; internal set; } = new();

        [JsonConstructor] public ModelCatalogueEntry() { }

        internal static ModelCatalogueEntry Create(IModelData modelData)
        {
            if (modelData == null) throw new ArgumentNullException(nameof(modelData), "Model data cannot be null.");
            ModelCatalogueEntry entry = new(modelData);
            entry = ModelMetaUtil.ResolveMeta(entry);
            return entry.FixDataFormats();
        }

        private ModelCatalogueEntry FixDataFormats()
        {
            Description = TextSplitter.SplitToParagraphs(Description);

            if (Name != null)
            {
                if (Name.Contains("(Legacy)"))
                {
                    Name = Name.Replace("(Legacy)", "").Trim();
                    IsLegacy = true;
                }

                if (Api == Api.Ollama) Name = ModelNameResolver.RemoveColonPrefix(Name, 0);
                else if (Api == Api.OpenRouter) Name = ModelNameResolver.RemoveColonPrefix(Name, 1);
            }

            return this;
        }

        internal void SetPrices(ModelPrice[] prices)
        {
            if (prices == null) return;

            foreach (ModelPrice price in prices)
            {
                if (price == null) continue;
                Pricing.AddOrUpdate(price.type, price.cost);
            }
        }

        internal ModelPrice[] GetPrices()
        {
            List<ModelPrice> prices = new();
            foreach (KeyValuePair<UsageType, double> pair in Pricing)
            {
                if (pair.Value == 0) continue;
                prices.Add(new ModelPrice(pair.Key, pair.Value));
            }
            return prices.ToArray();
        }

        private ModelCatalogueEntry(IModelData modelData)
        {
            Api = modelData.Api;
            Id = modelData.Id;
            Name = modelData.Name;
            Provider = modelData.Provider;
            Family = modelData.Family;
            Version = modelData.Version;
            Description = modelData.Description;
            OwnedBy = modelData.OwnedBy;
            CreatedAt = modelData.CreatedAt;

            InputTokenLimit = modelData.InputTokenLimit;
            OutputTokenLimit = modelData.OutputTokenLimit;

            IsFineTuned = modelData.IsFineTuned == true;

            BaseId = modelData.BaseId;

            if (modelData.Capability != null) Capability = modelData.Capability.Value;
            if (modelData.InputModality != null) InputModality = modelData.InputModality.Value;
            if (modelData.OutputModality != null) OutputModality = modelData.OutputModality.Value;

            Pricing[UsageType.PerCharacter] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerCharacter);
            Pricing[UsageType.Image] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerImage);
            Pricing[UsageType.InputCacheRead] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerInputCacheRead);
            Pricing[UsageType.InputCacheWrite] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerInputCacheWrite);
            Pricing[UsageType.PerRequest] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerRequest);
            Pricing[UsageType.WebSearch] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerWebSearch);
            Pricing[UsageType.InternalReasoning] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerInternalReasoning);
            Pricing[UsageType.PerMinute] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerMinute);
            Pricing[UsageType.InputToken] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerInputToken);
            Pricing[UsageType.OutputToken] = ModelMetaUtil.ResolveCost(Api, modelData.CostPerOutputToken);

            // remove 0 values from the dictionary
            for (int i = Pricing.Count - 1; i >= 0; i--)
            {
                KeyValuePair<UsageType, double> pair = Pricing.ElementAt(i);
                if (pair.Value == 0) Pricing.Remove(pair.Key);
            }

            (string family, string familyVersion) = ModelMetaUtil.ResolveFamily(Api, Id, Name);

            if (!string.IsNullOrWhiteSpace(family)) Family = family;
            if (!string.IsNullOrWhiteSpace(familyVersion)) FamilyVersion = familyVersion;
        }

        public bool Equals(ModelCatalogueEntry other) => other != null && Id == other.Id;
    }
}