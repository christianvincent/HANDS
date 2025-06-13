using Cysharp.Threading.Tasks;
using Glitch9.IO.Networking;
using Glitch9.ScriptableObjects;
using UnityEngine;

namespace Glitch9.AIDevKit.Ollama
{
    [AssetPath(AIDevKitConfig.CreatePath)]
    public class OllamaSettings : ScriptableResource<OllamaSettings>
    {
        // general settings
        [SerializeField] private string endpoint = "localhost";
        [SerializeField] private int port = 11434;

        // default models
        [SerializeField] private string defaultModel;
        [SerializeField] private bool enableOllama = false;

        public static string DefaultModel => Instance.defaultModel;

        public static string GetEndpoint()
        {
            string endpoint = Instance.endpoint;
            if (string.IsNullOrEmpty(endpoint)) endpoint = "localhost";
            int port = Instance.port;
            if (port <= 0) port = 11434;

            return $"http://{endpoint}:{port}";
        }

        public static UniTask<bool> CheckConnectionAsync()
        {
            if (!Instance.enableOllama) return UniTask.FromResult(false);
            string endpoint = GetEndpoint();
            string url = $"{endpoint}/api/tags"; // 가장 가벼운 API
            return NetworkUtil.CheckUrlAsync(url);
        }

        public static bool IsDefaultModel(string id, ModelFeature cap)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            if (cap.HasFlag(ModelFeature.TextGeneration) && id == DefaultModel) return true;

            return false;
        }
    }
}