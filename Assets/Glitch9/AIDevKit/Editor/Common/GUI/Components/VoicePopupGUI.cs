using System.Collections.Generic;
using Glitch9.AIDevKit.Editor;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    internal class VoicePopupGUI : AssetPopupGUI<Voice, VoiceFilter>
    {
        protected override Dictionary<Api, List<Voice>> GetFilteredAssets(VoiceFilter filter) => VoiceLibrary.GetFilteredRefs(filter);
        protected override Voice GetDefaultAssetId(VoiceFilter filter) => GetDefaultVoiceId(filter.Api);
        private static string GetDefaultVoiceId(Api api)
        {
            return api switch
            {
                Api.OpenAI => AIDevKitConfig.kDefault_OpenAI_Voice,
                Api.ElevenLabs => AIDevKitConfig.kDefault_ElevenLabs_Voice,
                _ => AIDevKitConfig.kDefault_OpenAI_Voice,
            };
        }

        protected override void DrawLibraryButton(GUIStyle style, float width)
        {
            if (GUILayout.Button(AIDevKitIcons.SpeechToText, style, GUILayout.Width(width)))
            {
                VoiceCatalogueWindow.ShowWindow();
            }
        }
    }
}