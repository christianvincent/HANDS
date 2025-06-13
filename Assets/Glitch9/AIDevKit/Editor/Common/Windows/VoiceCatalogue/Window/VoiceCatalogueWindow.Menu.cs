using Glitch9.Editor.IMGUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Glitch9.Internal;
using Cysharp.Threading.Tasks;

namespace Glitch9.AIDevKit.Editor
{
    public partial class VoiceCatalogueWindow
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
            await VoiceCatalogue.Instance.UpdateCatalogueAsync();
            TreeView.ReloadTreeView(true, true);
        }

        private async void UpdateElevenLabsCustomVoicesAsync()
        {
            await VoiceCatalogue.Instance.UpdateElevenLabsCustomVoicesAsync();
            TreeView.ReloadTreeView(true, true);
        }

        private void DrawFileMenu(Rect rect)
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Update Voice Catalogue"), false, UpdateCatalogue);
            menu.AddItem(new GUIContent("Update ElevenLabs Voice Library"), false, UpdateElevenLabsCustomVoicesAsync);

            menu.AddSeparator(string.Empty);

            menu.AddItem(new GUIContent("Generate OpenAI Voice Snippets"), false, () => VoiceSnippetGenerator.Generate(Api.OpenAI));
            menu.AddItem(new GUIContent("Generate ElevenLabs Voice Snippets"), false, () => VoiceSnippetGenerator.Generate(Api.ElevenLabs));

            menu.DropDown(rect);
        }


        private void DrawHelpMenu(Rect rect)
        {
            GenericMenu menu = new();

            menu.AddItem(GUIContents.OnlineDocument, false, () => Application.OpenURL(AIDevKitEditor.OnlineDocUrl));
            menu.AddItem(GUIContents.JoinDiscord, false, () => Application.OpenURL(EditorConfig.DiscordUrl));

            // https://www.openai.fm/ (An interactive demo for developers to try the new text-to-speech model in the OpenAI API.)
            menu.AddItem(new GUIContent("Official OpenAI TTS Demo"), false, () => Application.OpenURL("https://www.openai.fm/"));


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
            List<Voice> inMyLibrary = VoiceLibrary.ToList();
            if (inMyLibrary.IsNullOrEmpty())
            {
                Debug.LogWarning("No Model Assets found in the library.");
                return;
            }

            foreach (Voice voice in inMyLibrary)
            {
                if (voice == null)
                {
                    Debug.LogWarning("Model is null. Skipping.");
                    continue;
                }

                VoiceCatalogueEntry serverData = VoiceCatalogue.Instance.GetEntry(voice.Id);
                if (serverData == null)
                {
                    Debug.LogWarning($"Voice {voice.Id} not found in the catalogue. Skipping.");
                    continue;
                }

                voice.SetData(
                    api: serverData.Api,
                    id: serverData.Id,
                    name: serverData.Name,
                    gender: serverData.Gender,
                    age: serverData.Age,
                    language: serverData.Language
                );

                Debug.Log($"Updated {voice.Id} model asset.");
            }
        }
    }
}