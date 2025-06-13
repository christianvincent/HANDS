using Cysharp.Threading.Tasks;
using Glitch9.Editor.IMGUI;
using Glitch9.Internal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    public partial class ModelCatalogueWindow
    {
        protected override IEnumerable<ITreeViewMenuEntry> CreateMenuEntries()
        {
            yield return new TreeViewMenuDropdown("File", DrawFileMenu);
            yield return new TreeViewMenuDropdown("Help", DrawHelpMenu);
            yield return new TreeViewMenuDropdown("Debug", DrawDebugMenu);
            yield return new TreeViewMenuDropdown("Preferences", (_) => AIDevKitEditor.ShowPreferencesWindow());
            if (!AIDevKitConfig.IsPro) yield return new TreeViewMenuDropdown("Upgrade to Pro", (_) => AIDevKitEditor.OpenProURL());
            yield return new TreeViewMenuSearchField();
        }

        private async void UpdateCatalogue()
        {
            await ModelCatalogue.Instance.UpdateCatalogueAsync();
            TreeView.ReloadTreeView(true, true);
        }

        private void DrawFileMenu(Rect rect)
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Update Model Catalogue"), false, UpdateCatalogue);

            menu.AddSeparator(string.Empty);

            menu.AddItem(new GUIContent("Generate OpenAI Model Snippets"), false, () => ModelSnippetGenerator.Generate(Api.OpenAI));
            menu.AddItem(new GUIContent("Generate Google Model Snippets"), false, () => ModelSnippetGenerator.Generate(Api.Google));
            menu.AddItem(new GUIContent("Generate ElevenLabs Model Snippets"), false, () => ModelSnippetGenerator.Generate(Api.ElevenLabs));
            menu.AddItem(new GUIContent("Generate Ollama Model Snippets"), false, () => ModelSnippetGenerator.Generate(Api.Ollama));
            menu.AddItem(new GUIContent("Generate OpenRouter Model Snippets"), false, () => ModelSnippetGenerator.Generate(Api.OpenRouter));

            menu.DropDown(rect);
        }

        private void DrawHelpMenu(Rect rect)
        {
            GenericMenu menu = new();

            menu.AddItem(GUIContents.OnlineDocument, false, () => Application.OpenURL(AIDevKitEditor.OnlineDocUrl));
            menu.AddItem(GUIContents.JoinDiscord, false, () => Application.OpenURL(EditorConfig.DiscordUrl));

            menu.DropDown(rect);
        }

        private void DrawDebugMenu(Rect rect)
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Reload Assets (ScriptableObjects)"), false, ModelLibrary.FindAssets);
            menu.AddItem(new GUIContent("Remove Invalid Assets (ScriptableObjects)"), false, ModelLibrary.RemoveInvalidEntries);
            menu.AddItem(new GUIContent("Update Assets (ScriptableObjects)"), false, UpdateAssets);

            menu.AddSeparator(string.Empty);

            menu.AddItem(new GUIContent("Remove Duplicates from Catalogue"), false, () =>
            {
                ModelCatalogue.Instance.RemoveDuplicates();
                TreeView.ReloadTreeView(true, true);
            });

            menu.AddItem(new GUIContent("Run Startup Catalogue Update"), false, () =>
            {
                ModelCatalogue.Instance.CheckForUpdatesAsync(true).Forget();
            });

            menu.AddSeparator(string.Empty);

            menu.AddItem(new GUIContent("Force Update Model Dropdown"), false, () =>
            {
                ModelPopupGUI.ForceUpdateCache();
            });

            menu.DropDown(rect);
        }

        private void UpdateAssets()
        {
            List<Model> inMyLibrary = ModelLibrary.ToList();
            if (inMyLibrary.IsNullOrEmpty())
            {
                Debug.LogWarning("No Model Assets found in the library.");
                return;
            }

            foreach (Model model in inMyLibrary)
            {
                if (model == null)
                {
                    Debug.LogWarning("Model is null. Skipping.");
                    continue;
                }

                ModelCatalogueEntry serverData = ModelCatalogue.Instance.GetEntry(model.Id);
                if (serverData == null)
                {
                    Debug.LogWarning($"Model {model.Id} not found in the catalogue. Skipping.");
                    continue;
                }

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

                Debug.Log($"Updated {model.Id} model asset.");
            }
        }
    }
}