using System.Collections.Generic;

namespace Glitch9.Editor
{
    public enum TextBlockType
    {
        Text,
        CodeBlock,
        Header,
        Quote,
        UList,
        Hyperlink,
    }

    public partial class TextBlock
    {
        public string language;
        public string content;
        public TextBlockType type;
        public int headerLevel;
        public string arg;
        public bool IsCopied { get; set; }
        public bool IsEmpty => string.IsNullOrEmpty(content);

        public static TextBlock CodeBlock(string language, string code)
        {
            return new TextBlock
            {
                language = language,
                content = code,
                type = TextBlockType.CodeBlock
            };
        }

        public static TextBlock Text(string text) // plain text
        {
            return new TextBlock
            {
                content = text,
                type = TextBlockType.Text,
            };
        }

        public static TextBlock Header(string content, int headerLevel = 0)
        {
            return new TextBlock
            {
                content = content,
                type = TextBlockType.Header,
                headerLevel = headerLevel
            };
        }

        public static TextBlock Quote(string content)
        {
            return new TextBlock
            {
                content = content,
                type = TextBlockType.Quote
            };
        }

        public static TextBlock UListHeader(string content)
        {
            return new TextBlock
            {
                content = content,
                type = TextBlockType.Header,
                headerLevel = 0
            };
        }

        public static TextBlock UList(string content)
        {
            return new TextBlock
            {
                content = content,
                type = TextBlockType.UList
            };
        }

        public static TextBlock Hyperlink(string content, string url)
        {
            return new TextBlock
            {
                content = content,
                type = TextBlockType.Hyperlink,
                arg = url
            };
        }

        public static void DrawBlocks(List<TextBlock> textBlocks, float maxWidth)
        {
            TextBlock prevBlock = null;
            foreach (TextBlock textBlock in textBlocks)
            {
                TextBlockRenderer.Draw(textBlock, prevBlock, maxWidth);
                prevBlock = textBlock;
            }
        }
    }
}