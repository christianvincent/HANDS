using Glitch9.ScriptableObjects;
using UnityEngine;

namespace Glitch9.AIDevKit.Client
{
    /// <summary>
    /// Base class for AI client settings.
    /// This class is used to store API keys and other settings related to AI clients.
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    public abstract class AIClientSettings<TSelf> : ScriptableResource<TSelf>
        where TSelf : AIClientSettings<TSelf>
    {
        // [SerializeField, FormerlySerializedAs("apiKey")] protected string oldApiKey;
        // [SerializeField] protected string encryptedApiKey;
        // [SerializeField] protected bool encryptApiKey;
        [SerializeField] protected ApiKey apiKey;

        /// <summary>
        /// Retrieves the API key.
        /// </summary>
        public string GetApiKey() => apiKey?.GetKey();

        /// <summary>
        /// Checks if the API key is set.
        /// </summary> 
        public virtual bool HasApiKey() => apiKey != null && apiKey.HasValue;


        // #if UNITY_EDITOR
        //         private void OnValidate()
        //         {
        //             // 자동 마이그레이션
        //             if (apiKey == null || !apiKey.HasValue)
        //             {
        //                 bool hasOld = !string.IsNullOrEmpty(oldApiKey) || !string.IsNullOrEmpty(encryptedApiKey);
        //                 if (hasOld)
        //                 {
        //                     Debug.Log("Migrating legacy API key to ApiKey object.");
        //                     var key = encryptApiKey ? encryptedApiKey : oldApiKey;
        //                     UnityEditor.EditorUtility.SetDirty(this); // 마이그레이션 후 저장 트리거
        //                     apiKey = new ApiKey(key, encryptApiKey, false);
        //                     UnityEditor.EditorApplication.delayCall += () => UnityEditor.AssetDatabase.SaveAssets(); // 저장
        //                 }
        //             }
        //         }
        // #endif
    }
}