using Newtonsoft.Json;

namespace Glitch9.AIDevKit
{
    public class WebSearchOptionsWrapper
    {
        /// <summary>
        /// Optional. This tool searches the web for relevant results to use in a response.
        /// Learn more about the web search tool.
        /// </summary>
        [JsonProperty("web_search_options")] public WebSearchOptions WebSearchOptions { get; set; }

        public WebSearchOptionsWrapper(WebSearchOptions webSearchOptions)
        {
            WebSearchOptions = webSearchOptions;
        }
    }

    public class WebSearchOptions
    {
        /// <summary>
        /// Optional. High level guidance for the amount of context window space to use for the search.
        /// One of low, medium, or high. medium is the default.
        /// </summary>
        [JsonProperty("search_context_size")] public SearchContextSize? SearchContextSize { get; set; }

        /// <summary>
        /// Optional. Approximate location parameters for the search.
        /// </summary>
        [JsonProperty("user_location")] public UserLocation UserLocation { get; set; }
    }

    public enum SearchContextSize
    {
        Low,
        Medium,
        High
    }

    public class UserLocation
    {
        /// <summary>
        /// Required. Approximate location parameters for the search.
        /// </summary>
        [JsonProperty("approximate")] public ApproximateLocation Approximate { get; set; }
    }

    public class ApproximateLocation
    {
        /// <summary>
        /// Optional. Free text input for the city of the user, e.g. San Francisco.
        /// </summary>
        [JsonProperty("city")] public string City { get; set; }

        /// <summary>
        /// Optional. The two-letter ISO country code of the user, e.g. US.
        /// </summary>
        [JsonProperty("country")] public string Country { get; set; }

        /// <summary>
        /// Optional. Free text input for the region of the user, e.g. California.
        /// </summary>
        [JsonProperty("region")] public string Region { get; set; }

        /// <summary>
        /// Optional. The IANA timezone of the user, e.g. America/Los_Angeles.
        /// </summary>
        [JsonProperty("timezone")] public string Timezone { get; set; }

        /// <summary>
        /// Required. The type of location approximation. Always approximate.
        /// </summary>
        [JsonProperty("type")] public string Type { get; set; }
    }
}