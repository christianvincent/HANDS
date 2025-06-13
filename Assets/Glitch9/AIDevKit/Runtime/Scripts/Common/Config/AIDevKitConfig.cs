
using System.Collections.Generic;
using Glitch9.CoreLib.IO.Audio;

namespace Glitch9.AIDevKit
{
    internal class AIDevKitConfig
    {
        internal static bool IsPro = false;

        #region Default AIDevKit Settings 
        internal const bool ComponentGenerator = true;
        internal const bool ScriptDebugger = true;
        internal const int DefaultTimeoutInSeconds = 30;
        internal const double FreePriceMagicNumber = -1;
        internal const int DefaultRecordingDururationInSec = 30;
        internal const string RecordingFileNameFormat = "recording_{yyyyMMddHHmmss}.wav";
        internal const SampleRate DefaultRecordingSampleRate = SampleRate.Hz16000;
        internal const int DefaultRunPollingDelay = 5;
        internal const int DefaultRunPollingInterval = 2;
        internal const int DefaultRunPollingTimeout = 90;
        internal const int DefaultRequiredActionTimeoutSeconds = 60;
        internal const string DefaultProjectDescription =
            "Set in a futuristic city ruled by mega-corporations, players build relationships with quirky androids and rebellious hackers. " +
            "The localization system supports tone-aware translation and platform-specific phrasing for smooth, immersive dialogue.";

        #endregion Default AIDevKit Settings


        #region Default AI Models & Voices (for AIDevKit default settings and fallbacks) 

        // Written on 2025-05-01 by Munchkin
        internal const string kDefault_OpenAI_LLM = "gpt-4o";
        internal const string kDefault_OpenAI_IMG = "gpt-image-1";
        internal const string kDefault_OpenAI_TTS = "tts-1";
        internal const string kDefault_OpenAI_STT = "whisper-1";
        internal const string kDefault_OpenAI_EMB = "text-embedding-ada-002";
        internal const string kDefault_OpenAI_MOD = "omni-moderation-latest";
        internal const string kDefault_OpenAI_ASS = "gpt-4o";
        internal const string kDefault_OpenAI_RTM = "gpt-4o-realtime-preview";

        internal const string kDefault_OpenAI_Voice = "alloy";

        internal const string ID_DallE2 = "dall-e-2";
        internal const string ID_DallE3 = "dall-e-3";
        internal const string ID_GPT_Image_1 = "gpt-image-1";

        internal const string kDefault_Google_LLM = "models/gemini-2.0-flash";
        internal const string kDefault_Google_IMG = "models/gemini-2.0-flash-exp-image-generation";
        internal const string kDefault_Google_EMB = "models/embedding-001";
        internal const string kDefault_Google_VID = "models/veo-2.0-generate-001";

        internal const string kDefault_ElevenLabs_TTS = "eleven_flash_v2_5";
        internal const string kDefault_ElevenLabs_VCM = "eleven_english_sts_v2";
        internal const string kDefault_ElevenLabs_STT = "scribe_v1";
        internal const string kDefault_ElevenLabs_Voice = "21m00Tcm4TlvDq8ikWAM"; // Rachel

        // Editor Chat Summary Model
        internal const string kDefault_Chat_Model = "gpt-4o";
        internal const string kDefault_Chat_SummaryModel = "gpt-4o-mini";

        internal static readonly string[] kAllDefaultModels = new string[]
        {
            kDefault_OpenAI_LLM,
            kDefault_OpenAI_IMG,
            kDefault_OpenAI_TTS,
            kDefault_OpenAI_STT,
            kDefault_OpenAI_EMB,
            kDefault_OpenAI_MOD,
            kDefault_OpenAI_RTM,
            kDefault_Google_LLM,
            kDefault_Google_IMG,
            kDefault_Google_EMB,
            kDefault_ElevenLabs_TTS,
            kDefault_ElevenLabs_VCM,
            kDefault_Chat_Model,
            kDefault_Chat_SummaryModel
        };

        internal static readonly string[] kAllDefaultVoices = new string[]
        {
            kDefault_OpenAI_Voice,
            kDefault_ElevenLabs_Voice
        };

        internal static readonly Dictionary<string, ImageSize[]> ImageSizeOptions = new()
        {
            { AIDevKitConfig.ID_DallE2, new[] { ImageSize._1024x1024, ImageSize._256x256, ImageSize._512x512 } },
            { AIDevKitConfig.ID_DallE3, new[] { ImageSize._1024x1024, ImageSize._1024x1792, ImageSize._1792x1024 } },
            { AIDevKitConfig.ID_GPT_Image_1, new[] { ImageSize._1024x1024, ImageSize._1024x1536, ImageSize._1536x1024 } },
        };

        internal static readonly Dictionary<string, ImageQuality[]> ImageQualityOptions = new()
        {
            { AIDevKitConfig.ID_DallE2, new[] { ImageQuality.Standard } },
            { AIDevKitConfig.ID_DallE3, new[] { ImageQuality.Standard, ImageQuality.High } },
            { AIDevKitConfig.ID_GPT_Image_1, new[] { ImageQuality.Medium, ImageQuality.Low, ImageQuality.High } },
        };

        internal static readonly List<AudioEncoding> AudioEncodingOptions = new() { AudioEncoding.MP3, AudioEncoding.PCM };

        internal static ImageSize GetDefaultImageSizeForModel(string modelId)
        {
            if (ImageSizeOptions.TryGetValue(modelId, out ImageSize[] sizes))
            {
                return sizes[0]; // Return the first size as default
            }
            return ImageSize._1024x1024; // Fallback default size
        }

        internal static ImageQuality GetDefaultImageQualityForModel(string modelId)
        {
            if (ImageQualityOptions.TryGetValue(modelId, out ImageQuality[] qualities))
            {
                return qualities[0]; // Return the first quality as default
            }
            return ImageQuality.Standard; // Fallback default quality
        }

        internal static int ResolveMaxN(Model model)
        {
            const int defaultMaxN = 1;

            if (model == null) return defaultMaxN;
            if (string.IsNullOrEmpty(model.Id)) return defaultMaxN;
            if (model.IsDallE2()) return 10; // DALL-E 2 has a max N of 10
            if (model.IsDallE3()) return 1; // DALL-E 3 has a max N of 1
            if (model.IsGptImage1()) return 1; // GPT Image 1 has a max N of 1
            if (model.IsGemini()) return 1; // Gemini models typically have a max N of 1
            if (model.IsImagen()) return 4; // Imagen models typically have a max N of 4
            if (model.IsLLM()) return 20;

            return defaultMaxN;
        }


        #endregion Default AI Models   

        #region Instructions

        internal const string CodeGenInstruction =
            "Create a Unity C# script for Unity development. "
            + "Do not make any title, any explanations, transliteration or extra punctuation.";

        internal const string ComponentGenInstruction =
            "Create a Unity C# script that inherits from MonoBehaviour. "
            + "If necessary, include the default Start and Update methods with private visibility and no parameters. "
            + "Do not make any title, any explanations, transliteration or extra punctuation.";

        internal const string ComponentEditorInstruction =
            "You will be given a Unity C# script that inherits from MonoBehaviour and instructions to edit it. "
            + "Your task is to modify the script according to the instructions provided. "
            + "Do not make any title, any explanations, transliteration or extra punctuation.";

        #endregion Instructions


        #region Completion Request Config 

        internal const float TemperatureDefault = 1f;
        internal const float TemperatureMin = 0f;
        internal const float TemperatureMax = 2f;
        internal const float TopPDefault = 1f;
        internal const float TopPMin = 0f;
        internal const float TopPMax = 1f;

        internal const float FrequencyPenaltyDefault = 0f;
        internal const float FrequencyPaneltyMin = -2f;
        internal const float FrequencyPenaltyMax = 2f;

        #endregion Completion Request Config

        #region TTS Request Config

        internal const float kVoiceSpeedDefault = 1f;
        internal const float kVoiceSpeedMin = 0.25f;
        internal const float kVoiceSpeedMax = 4f;

        #endregion TTS Request Config  

        internal const string CreatePath = "Resources/AIDevKit";
    }
}