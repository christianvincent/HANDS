using System.IO;
using Glitch9.IO.Networking.RESTApi;
using Glitch9.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Glitch9.AIDevKit
{
    [AssetPath(AIDevKitConfig.CreatePath)]
    public class AIDevKitSettings : ScriptableResource<AIDevKitSettings>
    {
        // Default Models
        [SerializeField] private string defaultLLM = AIDevKitConfig.kDefault_OpenAI_LLM;
        [SerializeField] private string defaultIMG = AIDevKitConfig.kDefault_OpenAI_IMG;
        [SerializeField] private string defaultTTS = AIDevKitConfig.kDefault_OpenAI_TTS;
        [SerializeField] private string defaultSTT = AIDevKitConfig.kDefault_OpenAI_STT;
        [SerializeField] private string defaultEMB = AIDevKitConfig.kDefault_OpenAI_EMB;
        [SerializeField] private string defaultMOD = AIDevKitConfig.kDefault_OpenAI_MOD;
        [SerializeField] private string defaultVID = AIDevKitConfig.kDefault_Google_VID;

        // Path Settings
        [SerializeField, FormerlySerializedAs("runtimeOutputPath")] private string outputPath;
        [SerializeField] private bool checkForModelUpdatesOnStartup = true;
        [SerializeField] private bool promptHistoryOnRuntime = true;

        // Script Generator (CodeGen) Settings
        [SerializeField] private bool componentGenerator = AIDevKitConfig.ComponentGenerator;
        [SerializeField] private bool scriptDebugger = AIDevKitConfig.ScriptDebugger;

        // Log Levels
        [SerializeField] private RESTLogLevel logLevel = RESTLogLevel.RequestEndpoint | RESTLogLevel.ResponseBody;
        [SerializeField] private int requestTimeout = AIDevKitConfig.DefaultTimeoutInSeconds;

        // Project Context
        [SerializeField] private ProjectContext projectContext = new();

        // Static GetSetters ------------------------------------------------------------------------------------------------

        /// <summary>
        /// Default path for runtime downloads, used in dynamically updating or adding resources at runtime.
        /// </summary>
        public static string OutputPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Instance.outputPath)
                    || !Instance.outputPath.Contains(Application.persistentDataPath))
                {
                    Instance.outputPath = Path.Combine(Application.persistentDataPath, "Generated");
                }

                if (!Directory.Exists(Instance.outputPath)) Directory.CreateDirectory(Instance.outputPath);
                return Instance.outputPath;
            }
        }

        /// <summary>
        /// Enables or disables the component generator, which facilitates the creation of new Unity components from specified prompts.
        /// </summary>
        internal static bool EnableComponentGenerator => Instance.componentGenerator;
        internal static bool EnableScriptDebugger => Instance.scriptDebugger;
        public static RESTLogLevel LogLevel => Instance.logLevel;
        public static int RequestTimeout => Instance.requestTimeout;
        public static ProjectContext ProjectContext => Instance.projectContext;
        public static bool CheckForModelUpdatesOnStartup => Instance.checkForModelUpdatesOnStartup;
        public static string DefaultLLM => Instance.defaultLLM;
        public static string DefaultIMG => Instance.defaultIMG;
        public static string DefaultTTS => Instance.defaultTTS;
        public static string DefaultSTT => Instance.defaultSTT;
        public static string DefaultEMB => Instance.defaultEMB;
        public static string DefaultMOD => Instance.defaultMOD;
        public static string DefaultVID => Instance.defaultVID;
        public static bool PromptHistoryOnRuntime => Instance.promptHistoryOnRuntime;
    }
}