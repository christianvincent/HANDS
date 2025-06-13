using System.Collections.Generic;
using Glitch9.Editor;
using UnityEditor;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    internal class ModelCatalogueUpdateWindow : EditorWindow
    {
        const float kButtonWidth = 140f;
        internal static void ShowWindow(List<IModelData> newModels, List<ModelCatalogueEntry> deprecatedModels)
        {
            ModelCatalogueUpdateWindow popup = GetWindow<ModelCatalogueUpdateWindow>(true, "Model Updates", true);
            popup.Initialize(newModels, deprecatedModels);
        }

        private List<IModelData> _newModels;
        private List<ModelCatalogueEntry> _deprecatedModels;
        private List<ModelCatalogueEntry> _deprecatedModelsToRemove;
        private Dictionary<string, string> _modelNameCache = new();
        private Vector2 _scrollPosition;

        private void Initialize(List<IModelData> newModels, List<ModelCatalogueEntry> deprecatedModels)
        {
            _newModels = newModels;
            _deprecatedModels = deprecatedModels;
            _deprecatedModelsToRemove = new();
        }

        private string GetCachedNameOrResolve(string id)
        {
            if (_modelNameCache.TryGetValue(id, out string name))
            {
                return name;
            }

            name = ModelNameResolver.ResolveFromId(id);
            _modelNameCache[id] = name;
            return name;
        }

        private void OnGUI()
        {
            try
            {
                GUILayout.BeginVertical(ExStyles.paddedArea);
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
                    {
                        if (_newModels.Count > 0)
                        {
                            GUILayout.Label($"New Models ({_newModels.Count})", ExStyles.bigBoldLabel);

                            GUILayout.BeginVertical(ExStyles.helpBoxedSection);
                            {
                                foreach (IModelData model in _newModels)
                                {
                                    DrawNewModel(model);
                                }
                            }
                            GUILayout.EndVertical();
                        }

                        if (_deprecatedModels.Count > 0)
                        {
                            if (_newModels.Count > 0) GUILayout.Space(10f);

                            GUILayout.Label($"Deprecated Models ({_deprecatedModels.Count})", ExStyles.bigBoldLabel);

                            GUILayout.BeginVertical(ExStyles.helpBoxedSection);
                            {
                                foreach (ModelCatalogueEntry model in _deprecatedModels)
                                {
                                    DrawDeprecatedModel(model);
                                }
                            }
                            GUILayout.EndVertical();
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }
            finally
            {
                if (_deprecatedModelsToRemove.Count > 0)
                {
                    foreach (ModelCatalogueEntry model in _deprecatedModelsToRemove)
                    {
                        _deprecatedModels.Remove(model);
                    }
                    _deprecatedModelsToRemove.Clear();
                }
            }
        }

        private void DrawNewModel(IModelData model)
        {
            const float kHeight = 20f;

            GUILayout.BeginHorizontal(GUILayout.Height(kHeight));
            {
                // Layout: [Icon] [Name] [CreatedAt] [Status(legacy, deprecated)]
                Texture statusIcon = EditorIcons.StatusCheck;
                Texture apiIcon = AIDevKitGUIUtility.GetApiIcon(model.Api);
                string name = model.Name;
                if (string.IsNullOrEmpty(name)) name = GetCachedNameOrResolve(model.Id);

                GUILayout.Label(statusIcon, GUILayout.Width(20f), GUILayout.Height(kHeight));
                GUILayout.Label(apiIcon, GUILayout.Width(20f), GUILayout.Height(kHeight));
                GUILayout.Label(name, EditorStyles.label, GUILayout.ExpandWidth(true), GUILayout.Height(kHeight));
                GUILayout.Label(model.CreatedAt?.ToString("yyyy-MM-dd"), EditorStyles.label, GUILayout.Width(100f), GUILayout.Height(kHeight));

                if (GUILayout.Button("Add to Library", GUILayout.Width(kButtonWidth), GUILayout.Height(kHeight)))
                {
                    ModelCatalogueUtil.AddToLibrary(model.Id);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawDeprecatedModel(ModelCatalogueEntry model)
        {
            const float kHeight = 20f;

            GUILayout.BeginHorizontal(GUILayout.Height(kHeight));
            {
                // Layout: [Icon] [Name] [CreatedAt] [Status(legacy, deprecated)]
                Texture statusIcon = EditorIcons.StatusObsolete;
                Texture apiIcon = AIDevKitGUIUtility.GetApiIcon(model.Api);
                string name = model.Name;

                GUILayout.Label(statusIcon, GUILayout.Width(20f), GUILayout.Height(kHeight));
                GUILayout.Label(apiIcon, GUILayout.Width(20f), GUILayout.Height(kHeight));
                GUILayout.Label(name, EditorStyles.label, GUILayout.ExpandWidth(true), GUILayout.Height(kHeight));
                GUILayout.Label(model.CreatedAt?.ToString("yyyy-MM-dd"), EditorStyles.label, GUILayout.Width(100f), GUILayout.Height(kHeight));

                if (GUILayout.Button("Remove from Library", GUILayout.Width(kButtonWidth), GUILayout.Height(kHeight)))
                {
                    if (ModelCatalogueUtil.RemoveFromLibrary(model.Api, model.Id))
                    {
                        _deprecatedModelsToRemove.Add(model);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}