using Glitch9.CoreLib.IO.Audio;
using Newtonsoft.Json;

namespace Glitch9.AIDevKit
{
    public class SpeechOutputOptions
    {
        /// <summary>
        /// Parameters for audio output. 
        /// Required when audio output is requested with modalities: ["audio"].
        /// </summary>
        [JsonProperty("format")] public AudioEncoding? Format { get; set; }

        /// <summary>
        /// The voice the model uses to respond. Supported voices are alloy, ash, ballad, coral, echo, fable, nova, onyx, sage, and shimmer.
        /// </summary>
        [JsonProperty("voice")] public string Voice { get; set; }
    }
}