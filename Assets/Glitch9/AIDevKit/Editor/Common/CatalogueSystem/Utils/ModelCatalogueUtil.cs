using System.Linq;
using Cysharp.Threading.Tasks;
using Glitch9.AIDevKit.GENTasks;
using Glitch9.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    internal static class ModelCatalogueUtil
    {
        internal static bool AddToLibrary(ModelCatalogueTreeViewItem item)
        {
            if (item == null) return false;

            ModelCatalogueEntry entry = item.Data;
            if (entry == null) return false;

            string id = entry.Id;

            if (string.IsNullOrEmpty(id)) throw new System.Exception($"{typeof(Model).Name} ID is null or empty.");

            Model modelData = AddToLibrary(entry);
            if (modelData == null) throw new System.Exception($"Failed to add {entry.Api} {typeof(Model).Name} to {typeof(ModelLibrary).Name}.");
            item.InMyLibrary = true;

            return true;
        }

        internal static bool RemoveFromLibrary(ModelCatalogueTreeViewItem item)
        {
            if (item == null) return false;

            ModelCatalogueEntry entry = item.Data;
            if (entry == null) return false;

            item.InMyLibrary = !RemoveFromLibrary(entry.Api, entry.Id);

            return !item.InMyLibrary;
        }

        internal static void RemoveFromLibrary(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new System.Exception($"{typeof(Model).Name} ID is null or empty.");

            Model modelData = ModelLibrary.Get(id);
            if (modelData == null) throw new System.Exception($"Failed to remove {id} model from {typeof(ModelLibrary).Name}.");

            ModelLibrary.Remove(modelData);

            // ModelData is a Scriptable Object, so we need to delete the file itself.
            string path = AssetDatabase.GetAssetPath(modelData);
            if (!string.IsNullOrWhiteSpace(path)) AssetDatabase.DeleteAsset(path);

            ModelPopupGUI.ForceUpdateCache();
        }

        internal static void UpdateData(Model model, ModelCatalogueEntry serverData)
        {
            if (model == null) throw new System.Exception($"{typeof(Model).Name} inside {typeof(Model).Name} is null.");

            model.SetData(
                api: serverData.Api,
                id: serverData.Id,
                name: serverData.Name,
                capability: serverData.Capability,
                family: serverData.Family,
                inputModality: serverData.InputModality,
                outputModality: serverData.OutputModality,
                legacy: serverData.IsLegacy,
                familyVersion: serverData.FamilyVersion,
                inputTokenLimit: serverData.InputTokenLimit,
                outputTokenLimit: serverData.OutputTokenLimit,
                modelVersion: serverData.Version,
                fineTuned: serverData.IsFineTuned,
                prices: serverData.GetPrices()
            );
        }

        internal static void AddToLibrary(string id)
        {
            ModelCatalogueEntry serverData = ModelCatalogue.Instance.GetEntry(id)
                ?? throw new System.Exception($"Failed to retrieve {id} {typeof(Model).Name} from {typeof(ModelCatalogue).Name}.");
            AddToLibrary(serverData);
        }

        internal static Model AddToLibrary(ModelCatalogueEntry serverData)
        {
            string id = serverData.Id;
            if (string.IsNullOrWhiteSpace(id)) throw new System.Exception($"{typeof(Model).Name} ID is null or empty.");

            string internalResourcesPath = AIDevKitEditorPath.GetInternalResourcesPath();

            string targetDir = $"{internalResourcesPath}/Models";

            System.IO.Directory.CreateDirectory(targetDir);
            Model obj = ScriptableObject.CreateInstance<Model>();
            UpdateData(obj, serverData);

            string scriptableObjectName = ModelMetaUtil.RemoveSlashPrefix(id);
            scriptableObjectName = ScriptableObjectUtil.FixSOName(scriptableObjectName);

            string filePath = $"{targetDir}/{scriptableObjectName}.asset";
            Debug.Log($"Creating [{typeof(Model).Name}] Scriptable Object: " + filePath);

            AssetDatabase.CreateAsset(obj, filePath.FixDoubleAssets());
            EditorUtility.SetDirty(obj);
            ModelLibrary.Add(obj);
            Debug.Log($"Adding {obj} to {typeof(ModelLibrary).Name}...");

            ModelPopupGUI.ForceUpdateCache();

            return obj;
        }

        internal static bool RemoveFromLibrary(Api api, string id)
        {
            if (api == Api.None || api == Api.All || string.IsNullOrWhiteSpace(id)) return false;

            if (AIDevKitConfig.kAllDefaultModels.Contains(id))
            {
                Debug.LogWarning($"Cannot remove {api} {typeof(Model).Name} from {typeof(ModelLibrary).Name}. This is a default model.");
                return false;
            }

            RemoveFromLibrary(id);

            return true;
        }

        internal static async UniTask<bool> UpdateMetadataAsync(ModelCatalogueTreeViewItem item)
        {
            AIDevKitDebug.Blue($"Updating metadata for {item?.Data?.Name} ({item?.Data?.Id})...");

            if (item == null || item.IsInvalid())
            {
                Debug.LogWarning("Item is null or invalid. Cannot update metadata.");
                return false;
            }

            ModelCatalogueEntry entry = item.Data;
            if (entry == null)
            {
                Debug.LogWarning("Entry data is null. Cannot update metadata.");
                return false;
            }

            Api api = entry.Api;
            if (api == Api.None || api == Api.All)
            {
                Debug.LogWarning($"API is not set or is set to 'All' for {typeof(Model).Name} with ID: {entry.Id}. Cannot update metadata.");
                return false;
            }

            IModelData retrievedModelData = await GENTaskManager.RetrieveModelAsync(api, entry.Id);

            if (retrievedModelData == null)
            {
                Debug.LogWarning($"Failed to retrieve {api} {typeof(Model).Name} with ID: {entry.Id}");
                return false;
            }

            ModelCatalogueEntry newEntry = ModelCatalogue.Instance.CreateEntry(retrievedModelData);
            if (newEntry == null)
            {
                Debug.LogWarning($"Failed to create {typeof(ModelCatalogueEntry).Name} from retrieved data for {api} {typeof(Model).Name} with ID: {entry.Id}");
                return false;
            }

            ModelCatalogue.Instance.UpdateSingleEntry(newEntry);
            AIDevKitDebug.Blue($"Updated metadata for {api} {newEntry.Name} with ID: {entry.Id}");

            Model model = ModelLibrary.Get(entry.Id);
            if (model != null) UpdateData(model, newEntry);
            ModelPopupGUI.ForceUpdateCache();

            return true;
        }
    }
}