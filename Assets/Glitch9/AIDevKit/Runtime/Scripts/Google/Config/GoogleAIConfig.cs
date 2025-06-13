namespace Glitch9.AIDevKit.Google
{
    internal class GoogleAIConfig
    {
        internal const string BaseUrl = "https://generativelanguage.googleapis.com";
        internal const string Version = "v1";
        internal const string BetaVersion = "v1beta";
        internal const int MaxQuery = 10;
    }

    /// <summary>
    /// {URL}?{QueryParamsKey}={QueryParamsValue}
    /// Example: https://generativelanguage.googleapis.com/v1beta/{name=corpora/*}?updateMask=permissions
    /// </summary>
    internal class QueryParams
    {
        internal const string UPDATE_MASK = "updateMask";
    }

    /// <summary>
    /// {URL}:{Method}
    /// <para>Example: https://generativelanguage.googleapis.com/v1beta/{name=corpora/*}:query</para>
    /// </summary>
    internal class Methods
    {
        internal const string QUERY = "query";
        internal const string BATCH_CREATE = "batchCreate";
        internal const string BATCH_DELETE = "batchDelete";
        internal const string BATCH_UPDATE = "batchUpdate";
        internal const string BatchEmbedContents = "batchEmbedContents";
        internal const string COUNT_TOKENS = "countTokens";
        internal const string EmbedContent = "embedContent";
        internal const string GenerateAnswer = "generateAnswer";
        internal const string GenerateContent = "generateContent";
        internal const string GENERATE_TEXT = "generateText";
        internal const string StreamGenerateContent = "streamGenerateContent";
        internal const string TRANSFER_OWNERSHIP = "transferOwnership";

        // Added 2025.03.30
        internal const string Predict = "predict";

        // Added 2025.05.05
        internal const string PredictLongRunning = "predictLongRunning";
    }

    // All true for now
    internal class IsBeta
    {
        internal const bool CachedContents = true;
        internal const bool Corpora = true;
        internal const bool CorporaDocuments = true;
        internal const bool CorporaDocumentsChunks = true;
        internal const bool CorporaPermissions = true;
        internal const bool Files = true;
        internal const bool MediaUpload = true;
        internal const bool MediaUploadMetadata = true;
        internal const bool Models = true;
        internal const bool TunedModels = true;
        internal const bool TunedModelsPermissions = true;
    }
}