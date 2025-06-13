using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Glitch9.Editor
{
    public interface ITextBlockRenderer
    {
        void Draw(TextBlock block, TextBlock prevBlock, float maxWidth);
    }

    public class TextBlockRenderer
    {
        private static readonly Dictionary<TextBlockType, ITextBlockRenderer> _renderers = new()
        {
            { TextBlockType.Text, new PlainTextRenderer() },
            { TextBlockType.Header, new HeaderRenderer() },
            { TextBlockType.UList, new UListRenderer() },
            { TextBlockType.Quote, new QuoteRenderer() },
            { TextBlockType.CodeBlock, new CodeBlockRenderer() },
        };

        public static void Draw(TextBlock block, TextBlock prevBlock, float maxWidth)
        {
            if (block == null) return;

            if (_renderers.TryGetValue(block.type, out ITextBlockRenderer renderer))
            {
                renderer.Draw(block, prevBlock, maxWidth);
            }
        }
    }

    public class PlainTextRenderer : ITextBlockRenderer
    {
        public void Draw(TextBlock block, TextBlock prevBlock, float maxWidth)
        {
            string text = block.content.Trim();
            if (string.IsNullOrEmpty(text)) return;

            // if (prevBlock != null && prevBlock.type == TextBlockType.Header)
            // {
            //     // split with double \n (\n\n)
            //     string[] split = text.Split(new[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            //     if (split.Length > 1)
            //     {
            //         string firstLine = split[0].Trim();
            //         string restOfText = string.Join("\n", split, 1, split.Length - 1);

            //         TextBlockGUI.IndentedLabel(1, firstLine, TextBlockGUI.PlainText, GUILayout.MaxWidth(maxWidth));

            //         GUILayout.Space(10f);
            //         GUILayout.Label(restOfText, TextBlockGUI.PlainText, GUILayout.MaxWidth(maxWidth));
            //     }
            //     else
            //     {
            //         TextBlockGUI.IndentedLabel(1, text, TextBlockGUI.PlainText, GUILayout.MaxWidth(maxWidth));
            //     }
            // }
            // else
            // {
            //     GUILayout.Label(text, TextBlockGUI.PlainText, GUILayout.MaxWidth(maxWidth));
            // }

            GUILayout.Label(text, TextBlockGUI.PlainText, GUILayout.MaxWidth(maxWidth));
        }
    }

    public class HeaderRenderer : ITextBlockRenderer
    {
        public void Draw(TextBlock block, TextBlock prevBlock, float maxWidth)
        {
            const int kMaxTopMargin = 12;
            const int kMaxBotMargin = 6;

            int headerLevel = block.headerLevel;
            int topMargin = Mathf.Clamp((headerLevel + 1) * 10, 2, kMaxTopMargin);
            int botMargin = Mathf.Clamp((headerLevel + 1) * 2, 2, kMaxBotMargin);

            GUIStyle headerStyle = new(TextBlockGUI.PlainText)
            {
                fontSize = TextBlockGUI.PlainText.fontSize + headerLevel,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(0, 0, topMargin, botMargin),
            };

            //ExGUILayout.SelectableLabel(block.content, maxWidth, headerStyle);
            GUILayout.Label(block.content, headerStyle, GUILayout.MaxWidth(maxWidth));
        }
    }

    public class UListRenderer : ITextBlockRenderer
    {
        public void Draw(TextBlock block, TextBlock prevBlock, float maxWidth)
        {
            string text = block.content.Trim();
            if (string.IsNullOrEmpty(text)) return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(5f);
            GUILayout.Label("\u2022", TextBlockGUI.UListDot);
            //ExGUILayout.SelectableLabel(text, maxWidth, TextBlockGUI.UList);
            GUILayout.Label(text, TextBlockGUI.UList, GUILayout.MaxWidth(maxWidth));
            GUILayout.EndHorizontal();
        }
    }

    public class QuoteRenderer : ITextBlockRenderer
    {
        public void Draw(TextBlock block, TextBlock prevBlock, float maxWidth)
        {
            GUIStyle style = new(ExStyles.helpBox) { fontSize = 12, fontStyle = FontStyle.Italic };
            GUILayout.Label(block.content, style, GUILayout.MaxWidth(maxWidth));
        }
    }

    public class CodeBlockRenderer : ITextBlockRenderer
    {
        public void Draw(TextBlock block, TextBlock prevBlock, float maxWidth)
        {
            string text = block.content;
            GUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth));
            try
            {
                GUILayout.Space(5f);

                DrawHeader(text, block, maxWidth);
                DrawBody(text, block, maxWidth);
            }
            finally
            {
                GUILayout.EndVertical();
            }
        }

        private void DrawHeader(string text, TextBlock block, float maxWidth)
        {
            GUILayout.BeginHorizontal(TextBlockGUI.CodeBlockHeader, GUILayout.MaxWidth(maxWidth));
            try
            {
                GUILayout.Label(block.language, TextBlockGUI.HeaderLabel);
                GUILayout.FlexibleSpace();

                // save button
                GUIContent saveBtnLabel = new(EditorIcons.Save, "Save Code");
                if (GUILayout.Button(saveBtnLabel, TextBlockGUI.CodeBlockHeaderButton))
                {
                    string directory = Application.dataPath;
                    string extension = TextBlockUtil.GetExtension(block.language);
                    string path = EditorUtility.SaveFilePanel("Save Code", directory, "", extension);
                    if (!string.IsNullOrEmpty(path)) System.IO.File.WriteAllText(path, text);
                }

                GUILayout.Space(5f);

                GUIContent copyBtnLabel = new(EditorIcons.Clipboard, "Copy to Clipboard");//block.IsCopied ? new("\u2713 Copied!") : new("Copy Code");
                if (GUILayout.Button(copyBtnLabel, TextBlockGUI.CodeBlockHeaderButton))
                {
                    EditorGUIUtility.systemCopyBuffer = text;
                    block.IsCopied = true;
                }

                GUILayout.Space(5f);
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }

        private void DrawBody(string text, TextBlock block, float maxWidth)
        {
            string code;

            try
            {
                code = SyntaxHighlighter.Highlight(block.language, text);
            }
            catch
            {
                code = text;
            }

            //ExGUILayout.SelectableLabel(code, maxWidth, TextBlockGUI.CodeBlockContent);
            GUILayout.Label(code, TextBlockGUI.CodeBlockContent, GUILayout.MaxWidth(maxWidth));
        }
    }
}
