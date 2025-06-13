using UnityEditor;
using UnityEngine;

namespace Glitch9.Editor
{
    internal static class TextBlockGUI
    {
        private static readonly GUIStyleCache _cache = new();

        internal static GUIStyle PlainText => _cache.Get(nameof(PlainText), new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            richText = true,
            stretchWidth = true,
            stretchHeight = true,
            alignment = TextAnchor.UpperLeft,
        });

        internal static GUIStyle UList => _cache.Get(nameof(UList), new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            richText = true,
            margin = new RectOffset(0, 0, 0, 10),
            padding = new RectOffset(0, 0, 0, 10),
        });

        internal static GUIStyle UListDot => _cache.Get(nameof(UListDot), new GUIStyle(GUI.skin.label)
        {
            fixedWidth = 16,
            fixedHeight = 10,
        });

        internal static GUIStyle HeaderLabel => _cache.Get(nameof(HeaderLabel), new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            richText = true,
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(3, 3, 1, 1),
            margin = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.UpperLeft,
        });

        internal static GUIStyle CodeBlockHeaderButton => _cache.Get(nameof(CodeBlockHeaderButton), new GUIStyle(EditorStyles.iconButton)
        {
            fontSize = 10,
            fixedWidth = 20,
            fixedHeight = 18,
            alignment = TextAnchor.MiddleCenter,
        });

        internal static GUIStyle CodeBlockHeader => _cache.Get(nameof(CodeBlockHeader), new GUIStyle(EditorStyles.helpBox)
        {
            richText = true,
            normal = { background = CodeBlockHeaderTexture, textColor = new Color(0.8f, 0.8f, 0.8f) },
            fontSize = 10,
            fixedHeight = 22,
            margin = new RectOffset(0, 10, 0, 0),
            padding = new RectOffset(6, 6, 3, 3),
            stretchWidth = true,
        });

        internal static GUIStyle CodeBlockContent => _cache.Get(nameof(CodeBlockContent), new GUIStyle(EditorStyles.helpBox)
        {
            richText = true,
            margin = new RectOffset(0, 10, 0, 6),
            padding = new RectOffset(8, 8, 6, 6),
            normal = { background = CodeBlockBodyTexture, textColor = Color.white },
            fontSize = 12,
        });

        private const string kTextureDir = "{0}/CoreLib/Editor/Common/Gizmos/Textures/";
        private static readonly EditorTextureCache<Texture2D> _textureCache = new(string.Format(kTextureDir, EditorPathUtil.FindGlitch9Path()));

        internal static Texture2D CodeBlockBodyTexture => _textureCache.Get("codeblock-body.psd");
        internal static Texture2D CodeBlockHeaderTexture => _textureCache.Get("codeblock-header.psd");


        internal static void IndentedLabel(int indent, string text, GUIStyle style, params GUILayoutOption[] options)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (text.Length > 5)
            {
                char firstChar = text[0];
                char secondChar = text[1];

                if (firstChar.IsInteger() && secondChar == '.')
                {
                    GUILayout.Label(text, style, options);
                    return;
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(indent * 15f);
            GUILayout.Label(text, style, options);
            GUILayout.EndHorizontal();
        }

    }
}