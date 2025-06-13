using Glitch9.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    internal partial class AIDevKitGUI
    {
        // Shortcut for common GUI rendering tasks in AIDevKit.
        internal static class Presets
        {
            internal static void OfficialApiDocButton(Api api, string apiName, string docUrl)
            {
                Texture2D icon = AIDevKitGUIUtility.GetApiIcon(api);
                if (GUILayout.Button(new GUIContent($"  Official {apiName} Document", icon), GUILayout.Height(30f)))
                    Application.OpenURL(docUrl);
            }

            internal static void NoContentGenerated()
            {
                EditorGUILayout.LabelField("No content generated yet.", ExStyles.centeredBlueBoldLabel, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            }

            internal static void ReloadScreen(Action onReload)
            {
                GUILayout.FlexibleSpace(); // 화면 위쪽 여백 

                GUILayout.BeginHorizontal();
                try
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical(ExStyles.helpBox, GUILayout.Width(400)); // 원하는 너비 설정
                    {
                        GUILayout.Label("There was an error while loading the tool.\n\n" +
                                       "If you don't have necessary API key to use this tool," +
                                       "please set your API key in the user preferences.",
                                       ExStyles.bigLabel);

                        GUILayout.Space(20);


                        if (GUILayout.Button("Reload Window", ExStyles.bigButton))
                        {
                            onReload?.Invoke();
                        }

                        if (GUILayout.Button("Open Preferences", ExStyles.bigButton))
                        {
                            SettingsService.OpenUserPreferences(AIDevKitEditor.Providers.BasePath);
                        }

                    }
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                }
                finally
                {
                    GUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace(); // 화면 아래쪽 여백
            }

            internal static ImageSize DallE2ImageSizeField(ImageSize value)
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Size", GUILayout.Width(EditorGUIUtility.labelWidth - 2f));

                    if (ExGUILayout.ToggleLeft("256x256", value == ImageSize._256x256))
                    {
                        value = ImageSize._256x256;
                    }

                    if (ExGUILayout.ToggleMid("512x512", value == ImageSize._512x512))
                    {
                        value = ImageSize._512x512;
                    }

                    if (ExGUILayout.ToggleRight("1024x1024", value == ImageSize._1024x1024))
                    {
                        value = ImageSize._1024x1024;
                    }
                }
                GUILayout.EndHorizontal();

                return value;
            }

            internal static void DrawProRequiredWarning()
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                try
                {
                    GUILayout.Label(EditorIcons.ProBadge, GUILayout.Width(30), GUILayout.Height(30));
                    GUILayout.Label("This feature is only available in AI DevKit Pro.", ExStyles.statusBoxBigText, GUILayout.Height(30));
                    if (GUILayout.Button("Upgrade", GUILayout.Height(30), GUILayout.Width(100))) AIDevKitEditor.OpenProURL();
                }
                finally
                {
                    GUILayout.EndHorizontal();
                }
            }

            internal static void DrawUrlButtons(params (string, string)[] labelUrlPairs)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(15f); // for indentation
                    foreach (var (label, url) in labelUrlPairs)
                    {
                        if (GUILayout.Button(label, GUILayout.Height(Config.BigBtnHeight), GUILayout.Width(120), GUILayout.ExpandWidth(true)))
                        {
                            Application.OpenURL(url);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}