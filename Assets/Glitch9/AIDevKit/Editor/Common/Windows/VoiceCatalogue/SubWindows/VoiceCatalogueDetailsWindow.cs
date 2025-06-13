using Glitch9.Editor;
using Glitch9.Editor.IMGUI;
using UnityEditor;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    public partial class VoiceCatalogueWindow
    {
        public class VoiceCatalogueDetailsWindow : ExtendedTreeViewDetailsWindow
        {
            private string DateDisplay
            {
                get
                {
                    if (string.IsNullOrEmpty(_dateDisplay))
                    {
                        _dateDisplay = Data.CreatedAt?.ToString("yyyy-MM-dd");
                    }
                    return _dateDisplay;
                }
            }
            private string _dateDisplay;
            private string TypeDisplay
            {
                get
                {
                    if (string.IsNullOrEmpty(_typeDisplay))
                    {
                        using (StringBuilderPool.Get(out var sb))
                        {
                            sb.Append(Data.Type.GetInspectorName());

                            bool descriptiveExists = !string.IsNullOrEmpty(Data.Descriptive);
                            if (descriptiveExists)
                            {
                                sb.Append(" (");
                                sb.Append(Data.Descriptive.ToTitleCase());
                                sb.Append(")");
                            }

                            _typeDisplay = sb.ToString();
                        }
                    }
                    return _typeDisplay;
                }
            }
            private string _typeDisplay;
            private string _testPrompt;
            private bool _isLoading = false;


            private bool? _isFreeUserForElevenLabs;
            private bool IsFreeUserForElevenLabs
            {
                get
                {
                    if (_isFreeUserForElevenLabs == null) CheckIfFreeUserForElevenLabs();
                    return _isFreeUserForElevenLabs ?? false;
                }
            }
            private bool _isCheckingFreeUserForElevenLabs = false;

            private async void CheckIfFreeUserForElevenLabs()
            {
                if (_isFreeUserForElevenLabs != null) return;
                if (_isCheckingFreeUserForElevenLabs) return;
                _isCheckingFreeUserForElevenLabs = true;

                try
                {
                    _isFreeUserForElevenLabs = await AIDevKitEditor.IsElevenLabsFreeTierAsync();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to check Eleven Labs user status: {e.Message}");
                }
                finally
                {
                    _isCheckingFreeUserForElevenLabs = false;
                }
            }

            protected override float CalcTitleHeight() => 22f;

            protected override GUIContent CreateTitle()
            {
                string titleText = !string.IsNullOrEmpty(Data.Name) ? Data.Name : kFallbackTitle;
                return new GUIContent(titleText, AIDevKitGUIUtility.GetApiIcon(Data.Api));
            }

            protected override void DrawSubtitle()
            {
                TreeViewGUI.Subtitle($"Created At: {DateDisplay}", $"Owned by: {Data.OwnedBy ?? "Unknown"}");
            }

            protected override void DrawBody()
            {
                DrawLocalStatus();
                GUILayout.Space(5);
                DrawVoiceTest();
                GUILayout.Space(5);
                DrawVoiceDetails();

                if (!string.IsNullOrEmpty(Data.Description))
                {
                    GUILayout.Space(5);
                    DrawDescription();
                }

                // GUILayout.Space(5);
                // DrawUsageStats();
                // GUILayout.Space(5);
                // DrawStatusAndSettings();
                // GUILayout.Space(5);
                // DrawOwnerInfo();
            }

            private void DrawLocalStatus()
            {
                const float kHeight = 36f;

                if (!Item.IsFree && Item.Api == Api.ElevenLabs && IsFreeUserForElevenLabs)
                {
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        GUILayout.Label(EditorIcons.StatusObsolete, GUILayout.Width(30), GUILayout.Height(kHeight));
                        GUILayout.Label("This voice is not available for free users.", ExStyles.verticallyCenteredLabel, GUILayout.Height(kHeight));
                    }
                    GUILayout.EndHorizontal();
                    return;
                }

                Texture statusIcon;
                string message;

                if (Item.InMyLibrary)
                {
                    statusIcon = EditorIcons.StatusCheck;
                    message = "This voice is already in your library.";
                }
                else
                {
                    statusIcon = EditorIcons.StatusWarning;
                    message = "This voice is not in your library.";
                }

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    if (statusIcon != null)
                    {
                        GUILayout.Label(statusIcon, GUILayout.Width(30), GUILayout.Height(kHeight));
                    }

                    GUILayout.Label(message, ExStyles.verticallyCenteredLabel, GUILayout.Height(kHeight));

                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    {
                        if (Item.InMyLibrary)
                        {
                            GUI.color = ExGUI.red;
                            if (GUILayout.Button(GUIContents.RemoveFromLibrary, AIDevKitStyles.WordWrappedButton, GUILayout.Width(40), GUILayout.Height(kHeight)))
                            {
                                VoiceCatalogueUtil.RemoveFromLibrary(Item);
                                Repaint();
                            }
                            GUI.color = Color.white;
                        }
                        else
                        {

                            GUI.color = ExGUI.green;
                            if (GUILayout.Button(GUIContents.AddToLibrary, AIDevKitStyles.WordWrappedButton, GUILayout.Width(40), GUILayout.Height(kHeight)))
                            {
                                VoiceCatalogueUtil.AddToLibrary(Item);
                                Repaint();
                            }
                            GUI.color = Color.white;
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }

            private async void PlayVoiceTest()
            {
                if (Data.Api != Api.ElevenLabs)
                {
                    Debug.LogWarning("Voice test is only available for Eleven Labs voices for now.");
                    return;
                }

                if (string.IsNullOrEmpty(_testPrompt)) return;

                _isLoading = true;

                try
                {
                    var clip = await _testPrompt.GENSpeech()
                        .SetVoice(Data.Id)
                        .SetModel(AIDevKitConfig.kDefault_ElevenLabs_TTS)
                        .ExecuteAsync();

                    if (clip != null) EditorAudioPlayer.Play(clip);
                    else Debug.LogError("Failed to play voice test.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to play voice test: {e.Message}");
                }
                finally
                {
                    _isLoading = false;
                }
            }

            private void DrawVoiceTest()
            {
                TreeViewGUI.HeaderLabel("Voice Test");

                GUILayout.BeginVertical(ExStyles.helpBoxedSection);
                {
                    if (_isLoading)
                    {
                        GUILayout.Label("Loading...", ExStyles.verticallyCenteredLabel, GUILayout.Height(40), GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        _testPrompt = GUILayout.TextField(_testPrompt, ExStyles.textField, GUILayout.Height(40), GUILayout.ExpandWidth(true));

                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Play Preview", GUILayout.Width(100f))) TreeView.PlayPreviewUrl(Item);
                            if (GUILayout.Button("Play")) PlayVoiceTest();
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
            }

            private string _availableForTiersDisplay;

            private void DrawVoiceDetails()
            {
                TreeViewGUI.BeginSection("Voice Details");
                {
                    AIDevKitGUI.CopiableLabelField("Name", Data.Name);
                    AIDevKitGUI.LabelField("Gender", Data.Gender);
                    AIDevKitGUI.CopiableLabelField("Language", Item.LanguageDisplay);
                    AIDevKitGUI.LabelField("Age", Data.Age);
                    AIDevKitGUI.CopiableLabelField("Type", TypeDisplay);
                    AIDevKitGUI.LabelField("Category", Data.Category);
                    AIDevKitGUI.CopiableLabelField("Image URL", Data.ImageUrl);
                    AIDevKitGUI.LabelField("Is Free", Data.IsFree);

                    if (!Data.AvailableForTiers.IsNullOrEmpty())
                    {
                        if (_availableForTiersDisplay == null)
                        {
                            using (StringBuilderPool.Get(out var sb))
                            {
                                foreach (var tier in Data.AvailableForTiers)
                                {
                                    sb.AppendLine(tier);
                                }
                                _availableForTiersDisplay = sb.ToString();
                            }
                        }
                        AIDevKitGUI.CopiableLabelField("Available For Tiers", _availableForTiersDisplay);
                    }
                }
                TreeViewGUI.EndSection();
            }

            private void DrawDescription()
            {
                TreeViewGUI.HeaderLabel("Voice Description");

                GUILayout.BeginVertical(ExStyles.helpBoxedSection);
                {
                    EditorGUILayout.LabelField(Data.Description, EditorStyles.wordWrappedLabel);
                }
                GUILayout.EndVertical();
            }

            // private void DrawUsageStats()
            // {
            //     TreeViewGUI.HeaderLabel("Usage Stats");

            //     GUILayout.BeginVertical(ExEditorStyles.helpBoxedSection);
            //     {
            //         AIDevKitGUI.LabelField("Playback Rate", Data.PlaybackRate);
            //         AIDevKitGUI.LabelField("Usage (1 Year)", Data.UsageCharacterCount1Y);
            //         AIDevKitGUI.LabelField("Usage (7 Days)", Data.UsageCharacterCount7D);
            //         AIDevKitGUI.LabelField("Play API Usage (1 Year)", Data.PlayApiUsageCharacterCount1Y);
            //         AIDevKitGUI.LabelField("Cloned By Count", Data.ClonedByCount);
            //     }
            //     GUILayout.EndVertical();
            // }

            // private void DrawStatusAndSettings()
            // {
            //     TreeViewGUI.HeaderLabel("Status & Settings");

            //     GUILayout.BeginVertical(ExEditorStyles.helpBoxedSection);
            //     {
            //         AIDevKitGUI.LabelField("Live Moderation", Data.LiveModerationEnabled);
            //         AIDevKitGUI.LabelField("Featured", Data.Featured);
            //         AIDevKitGUI.LabelField("Added by User", Data.IsAddedByUser);
            //         AIDevKitGUI.LabelField("Notice Period (Days)", Data.NoticePeriod);
            //     }
            //     GUILayout.EndVertical();
            // }

            // private void DrawOwnerInfo()
            // {
            //     TreeViewGUI.HeaderLabel("Owner Info");

            //     GUILayout.BeginVertical(ExEditorStyles.helpBoxedSection);
            //     {
            //         AIDevKitGUI.LabelField("Instagram", Data.InstagramUsername);
            //         AIDevKitGUI.LabelField("Twitter", Data.TwitterUsername);
            //         AIDevKitGUI.LabelField("TikTok", Data.TikTokUsername);
            //         AIDevKitGUI.LabelField("YouTube", Data.YouTubeUsername);
            //     }
            //     GUILayout.EndVertical();
            // }
        }
    }
}