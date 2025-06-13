using Glitch9.Editor;
using UnityEditor;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    internal partial class AIDevKitGUI
    {
        private static class Config
        {
            internal const float BigBtnHeight = 24f;
        }

        internal static void LabelField<T>(string label, T? value) where T : struct
        {
            string display = value.HasValue ? AIDevKitGUIUtility.FormatValue(value.Value) : "-";
            EditorGUILayout.LabelField(label, display, AIDevKitStyles.Label);
        }

        internal static void LabelField<T>(string label, T value)
        {
            string display = AIDevKitGUIUtility.FormatValue(value);
            EditorGUILayout.LabelField(label, display, AIDevKitStyles.Label);
        }

        internal static void CopiableLabelField(string label, string value) => CopiableLabelField(label, new GUIContent(value));
        internal static void CopiableLabelField(string label, GUIContent value) => Render.DrawCopiableLabelField(label, value);
        internal static bool LinkButton(string label) => GUILayout.Button(label, AIDevKitStyles.LinkButton, GUILayout.Height(16));
        internal static void TokenField(string label, int? value) => Render.DrawTokenField(label, value);
        internal static void CurrencyField(string label, Currency value) => Render.DrawCurrencyField(label, value);
        internal static void OutputPathField(GUIContent label, SerializedProperty outputPath) => ExEditorGUI.PathField(label, outputPath, Application.persistentDataPath, AIDevKitSettings.OutputPath);
        internal static void OutputPathField(SerializedProperty outputPath) => ExEditorGUI.PathField(GUIContents.OutputPath, outputPath, Application.persistentDataPath, AIDevKitSettings.OutputPath);

        internal static ImageSize ImageSizePopup(ImageSize selected, Model model)
        {
            // check if the current model supports the selected image size
            if (model != null && !AIDevKitUtils.IsImageSizeSupported(selected, model))
            {
                selected = AIDevKitConfig.GetDefaultImageSizeForModel(model.Id);
            }

            return (ImageSize)EditorGUILayout.EnumPopup(
                label: GUIContents.ImageSize,
                selected: selected,
                checkEnabled: size => AIDevKitUtils.IsImageSizeSupported((ImageSize)size, model),
                includeObsolete: false
            );
        }

        internal static ImageQuality ImageQualityPopup(ImageQuality selected, Model model)
        {
            // check if the current model supports the selected image quality
            if (model != null && !AIDevKitUtils.IsImageQualitySupported(selected, model))
            {
                selected = AIDevKitConfig.GetDefaultImageQualityForModel(model.Id);
            }

            return (ImageQuality)EditorGUILayout.EnumPopup(
                label: GUIContents.ImageQuality,
                selected: selected,
                checkEnabled: quality => AIDevKitUtils.IsImageQualitySupported((ImageQuality)quality, model),
                includeObsolete: false
            );
        }
    }
}