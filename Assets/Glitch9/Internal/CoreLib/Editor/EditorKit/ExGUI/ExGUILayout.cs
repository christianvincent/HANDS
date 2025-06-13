using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Glitch9.Editor
{
    public enum MessageTypeEx { None, Success, Error, Warning, }
    public partial class ExGUILayout
    {
        #region Boxed Label
        public static void BoxedLabel(string label, params GUILayoutOption[] options) => BoxedLabel(new GUIContent(label), options);
        public static void BoxedLabel(GUIContent label, params GUILayoutOption[] options) => EditorGUILayout.LabelField(label, ExStyles.Box(TextAnchor.MiddleCenter), options);
        public static void ErrorLabel(string label) => BoxedLabel(new GUIContent(label, EditorIcons.StatusError), GUILayout.Height(18f));
        public static void ErrorLabel(string label1, string label2, params GUILayoutOption[] options) => ExGUIInternal.DrawErrorLabel(label1, label2, options);
        #endregion

        #region Icon Label - This is very frustrating, but Unity's GUIContent doesn't have a way to put little spaces between the icon and the text. 
        public static void IconLabel(string label, Texture icon, GUIStyle style = null) => IconLabel(new GUIContent(label), icon, 0, style);
        public static void IconLabel(GUIContent label, Texture icon, GUIStyle style = null) => IconLabel(new GUIContent(label), icon, 0, style);
        public static void IconLabel(string label, Texture icon, int space, GUIStyle style = null) => IconLabel(new GUIContent(label), icon, space, style);
        public static void IconLabel(GUIContent label, Texture icon, int space, GUIStyle style = null) => ExGUIInternal.DrawIconLabelWithSpace(label, icon, space, style);
        #endregion 

        #region Toggles  
        public static bool ToggleMid(string label, bool isOn, params GUILayoutOption[] options) => ToggleMid(new GUIContent(label), isOn, options);
        public static bool ToggleMid(GUIContent content, bool isOn, params GUILayoutOption[] options) => GUILayout.Toggle(isOn, content, EditorStyles.miniButtonMid, options);
        public static bool ToggleLeft(string label, bool isOn, params GUILayoutOption[] options) => ToggleLeft(new GUIContent(label), isOn, options);
        public static bool ToggleLeft(GUIContent content, bool isOn, params GUILayoutOption[] options) => GUILayout.Toggle(isOn, content, EditorStyles.miniButtonLeft, options);
        public static bool ToggleRight(string label, bool isOn, params GUILayoutOption[] options) => ToggleRight(new GUIContent(label), isOn, options);
        public static bool ToggleRight(GUIContent content, bool isOn, params GUILayoutOption[] options) => GUILayout.Toggle(isOn, content, EditorStyles.miniButtonRight, options);
        public static bool[] ToggleGroup(string label, string[] btnLabels, bool[] values, params GUILayoutOption[] options) => ExGUIInternal.DrawToggleGroup(new GUIContent(label), btnLabels, values, options);
        #endregion Toggles   

        #region Animated Toggle
        public static bool AnimatedToggle(string label, bool isOn, string uniqueKey, params GUILayoutOption[] options) => AnimatedToggle(new GUIContent(label), isOn, uniqueKey, options);
        public static bool AnimatedToggle(GUIContent content, bool isOn, string uniqueKey, params GUILayoutOption[] options) => ExGUIInternal.DrawAnimatedToggle(content, isOn, uniqueKey, options);
        public static bool AnimatedToggle(string label, bool isOn, params GUILayoutOption[] options) => AnimatedToggle(new GUIContent(label), isOn, null, options);
        public static bool AnimatedToggle(GUIContent content, bool isOn, params GUILayoutOption[] options) => ExGUIInternal.DrawAnimatedToggle(content, isOn, null, options);

        #endregion Animated Toggle

        #region Texture Field
        public static void TextureField(Texture texture, Vector2? size = null, float offsetY = 0) => ExGUIInternal.DrawTextureField(texture, size, offsetY);
        #endregion

        #region ColorPicker
        public static Color ColorPicker(string label, Color selected, IList<Color> displayedOptions) => ColorPicker(label, selected, displayedOptions);
        public static Color ColorPicker(GUIContent label, Color selected, IList<Color> displayedOptions) => ExGUIInternal.DrawColorPicker(label, selected, displayedOptions);
        #endregion ColorPicker

        #region Path Fields
        public static string PathField(string label, string value, string rootPath, string defaultPath) => PathField(new GUIContent(label), value, rootPath, defaultPath);
        public static string PathField(GUIContent label, string value, string rootPath, string defaultPath) => ExGUIInternal.DrawPathField(label, value, rootPath, defaultPath);
        #endregion Path Fields  

        #region Switches
        public static void TrueOrFalseSwitch(string label, Action<bool> action) => Switch(label, action, "True", "False");
        public static void SetOrUnsetSwitch(string label, Action<bool> action) => Switch(label, action, "Set", "Unset");
        public static void Switch(string label, Action<bool> action, string yes, string no) => ExGUIInternal.DrawSwitch(label, action, yes, no);
        public static void Switch(string label, params ButtonInfo[] buttonEntries) => ExGUIInternal.DrawSwitchFromButtonEntries(label, buttonEntries);
        #endregion Switches

        #region Enum Switches
        public static T EnumSwitch<T>(string label, T enumValue, int startIndex = 0, params GUILayoutOption[] options) where T : Enum => EnumSwitch(new GUIContent(label), enumValue, startIndex, options);
        public static T EnumSwitch<T>(GUIContent label, T enumValue, int startIndex = 0, params GUILayoutOption[] options) where T : Enum => ExGUIInternal.DrawEnumSwitch(label, enumValue, startIndex, options);
        public static T EnumFlagSwitch<T>(T enumValue, int startIndex = 1, params GUILayoutOption[] options) where T : Enum => ExGUIInternal.DrawEnumFlagSwitch(enumValue, startIndex, options);
        #endregion Enum Switches

        #region HelpBox (extended)
        public static void HelpBoxEx(string message, MessageTypeEx type, GUIStyle textStyle, params GUILayoutOption[] options) => ExGUIInternal.DrawHelpBoxEx(message, type, textStyle, options);
        public static void HelpBoxEx(string message, MessageTypeEx type, params GUILayoutOption[] options) => HelpBoxEx(message, type, ExStyles.statusBoxText, options);
        public static void HelpBoxExBig(string message, MessageTypeEx type, params GUILayoutOption[] options) => HelpBoxEx(message, type, ExStyles.statusBoxBigText, options);
        #endregion HelpBox (extended)

        #region NullableFields
        public static T? NullableField<T>(string label, T? value, T defaultValue, Func<T, T> drawer) where T : struct => NullableField(new GUIContent(label), value, defaultValue, drawer);
        public static T? NullableField<T>(GUIContent label, T? value, T defaultValue, Func<T, T> drawer) where T : struct => ExGUIInternal.DrawNullableField(label, value, defaultValue, drawer);
        public static T? NullableField<T>(T? value, T defaultValue, Func<T, T> drawer) where T : struct => ExGUIInternal.DrawNullableFieldWithoutLabel(value, defaultValue, drawer);
        public static double? NullableDoubleSlider(GUIContent label, double? value, float defaultValue, float min, float max) => ExGUIInternal.DrawNullableDoubleSlider(label, value, defaultValue, min, max);
        #endregion NullableFields

        #region ResettableField
        public static T ResettableField<T>(GUIContent label, T value, T defaultValue, Func<T, T> drawer) where T : struct => ExGUIInternal.DrawResettableField(label, value, defaultValue, drawer);
        public static T ResettableField<T>(string label, T value, T defaultValue, Func<T, T> drawer) where T : struct => ResettableField(new GUIContent(label), value, defaultValue, drawer);
        #endregion ResettableField

        #region ButtonField 
        public static bool ButtonField(string label) => ExGUIInternal.DrawButtonField(new GUIContent(label));
        public static bool ButtonField(GUIContent label) => ExGUIInternal.DrawButtonField(label);
        #endregion ButtonField

        #region ExpandableTextField 
        public static void ExpandableTextField(string label, string value, Action<string> onEdited, params GUILayoutOption[] options) => ExpandableTextField(new GUIContent(label), value, onEdited, ExStyles.wordWrappedTextField, options);
        public static void ExpandableTextField(GUIContent label, string value, Action<string> onEdited, params GUILayoutOption[] options) => ExpandableTextField(label, value, onEdited, ExStyles.wordWrappedTextField, options);
        public static void ExpandableTextField(string label, string value, Action<string> onEdited, GUIStyle style, params GUILayoutOption[] options) => ExpandableTextField(new GUIContent(label), value, onEdited, style, options);
        public static void ExpandableTextField(GUIContent label, string value, Action<string> onEdited, GUIStyle style, params GUILayoutOption[] options) => ExGUIInternal.DrawExpandableTextField(label, value, onEdited, style, options);
        #endregion ExpandableTextField

        #region Enum Popups
        public static T EnumPopup<T>(T selected, params GUILayoutOption[] options) where T : Enum => EnumPopup(GUIContent.none, selected, options);
        public static T EnumPopup<T>(string label, T selected, params GUILayoutOption[] options) where T : Enum => EnumPopup(new GUIContent(label), selected, options);
        public static T EnumPopup<T>(GUIContent label, T selected, params GUILayoutOption[] options) where T : Enum => ExGUIInternal.DrawEnumPopup(label, selected, options);
        public static T EnumPopupEx<T>(T selected, IEnumerable<T> displayedOptions, string[] displayedNames = null, GUIStyle style = null, params GUILayoutOption[] options) where T : Enum => EnumPopupEx(null, selected, displayedOptions, displayedNames, style, options);
        public static T EnumPopupEx<T>(string label, T selected, IEnumerable<T> displayedOptions, string[] displayedNames = null, GUIStyle style = null, params GUILayoutOption[] options) where T : Enum => ExGUIInternal.DrawExtendedEnumPopup(label, selected, displayedOptions, displayedNames, style, options);
        public static T ResettableEnumPopup<T>(string label, T selected, T defaultValue) where T : Enum => ResettableEnumPopup(new GUIContent(label), selected, defaultValue);
        public static T ResettableEnumPopup<T>(GUIContent label, T selected, T defaultValue) where T : Enum => ExGUIInternal.DrawResettableEnumPopup(label, selected, defaultValue);
        #endregion Enum Popups

        #region Text Popups
        public static string TextPopup(string value, string[] displayedOptions, params GUILayoutOption[] options) => ExGUIInternal.DrawPopupT(GUIContent.none, value, displayedOptions, options);
        public static string TextPopup(string label, string value, string[] displayedOptions, params GUILayoutOption[] options) => ExGUIInternal.DrawPopupT(new GUIContent(label), value, displayedOptions, options);
        public static string TextPopup(GUIContent label, string value, string[] displayedOptions, params GUILayoutOption[] options) => ExGUIInternal.DrawPopupT(label, value, displayedOptions, options);
        #endregion Text Popups

        #region Enum Toolbar
        public static bool EnumToolbar<T>(T enumValue, out T newEnum, GUIStyle toolbarStyle, params GUILayoutOption[] options) where T : Enum => ExGUIInternal.DrawEnumToolbar(enumValue, out newEnum, toolbarStyle, options);
        public static bool EnumToolbar<T>(T enumValue, out T newEnum, params GUILayoutOption[] options) where T : Enum => EnumToolbar(enumValue, out newEnum, null, options);
        #endregion Enum Toolbar

        #region Add Component Button
        public static bool AddComponentButton(string label) => AddComponentButton(new GUIContent(label));
        public static bool AddComponentButton(GUIContent label) => ExGUIInternal.DrawAddComponentButton(label);
        #endregion Add Component Button

        #region Layouts 
        internal static void BeginSection(string title) => BeginSection(new GUIContent(title));
        internal static void BeginSection(GUIContent title)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
        }
        internal static void BeginSection(string title, Action trailing) => BeginSection(new GUIContent(title), trailing);
        internal static void BeginSection(GUIContent title, Action trailing)
        {
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(title, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                trailing?.Invoke();
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = 1;
        }

        internal static void EndSection()
        {
            EditorGUI.indentLevel = 0;
            EditorGUILayout.Space();
        }
        #endregion Layouts
    }
}

