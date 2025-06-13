using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Glitch9.Editor
{
    public partial class TextBlock
    {
        private static readonly Dictionary<int, List<TextBlock>> _cachedTextBlocks = new();

        public static void DrawText(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return;

            int hashCode = text.GetHashCode();
            if (_cachedTextBlocks.TryGetValue(hashCode, out List<TextBlock> textBlocks))
            {
                DrawBlocks(textBlocks, maxWidth);
                return;
            }

            textBlocks = new List<TextBlock>();
            int lastIndex = 0;

            // STEP 1: 두 줄짜리 인덱스 블록 먼저 파싱
            // MatchCollection indexBlocks = Regex.Matches(text, @"(?<title>^\d+\.\s+.+)\r?\n(?<body>^\s+.+)", RegexOptions.Multiline);

            // foreach (Match match in indexBlocks)
            // {
            //     string title = match.Groups["title"].Value.Trim();
            //     string body = match.Groups["body"].Value.Trim();
            //     textBlocks.Add(TextBlock.Header(title));
            //     textBlocks.Add(TextBlock.Text(body, 1));
            // }

            // Use regex to find and process different types of blocks
            // MatchCollection matches = Regex.Matches(text, @"(###\s+(?<header>.+))|(>\s+(?<quote>.+))|(```(?<language>\w+)\s*(?<code>[\s\S]*?)\s*```)", RegexOptions.Multiline);

            // Regex Version 2
            MatchCollection matches = Regex.Matches(text,
                @"(###\s+(?<header>.+))" +
                @"|(>\s+(?<quote>.+))" +
                @"|(```(?<language>\w+)\s*(?<code>[\s\S]*?)\s*```)" +
                @"|(^\s*(?<ulist_header>\d+\.\s+\*\*.+?\*\*):)" +
                @"|^\s*[-*+]\s+(?<ulist>.+)",
                RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                int matchStartIndex = match.Index;

                // Add plain text before the match
                if (matchStartIndex > lastIndex)
                {
                    string plainText = text.Substring(lastIndex, matchStartIndex - lastIndex).Trim();
                    if (!string.IsNullOrEmpty(plainText)) textBlocks.Add(TextBlock.Text(TextBlockUtil.ProcessTags(plainText)));
                }

                if (match.Groups["header"].Success)
                {
                    string headerContent = match.Groups["header"].Value.Trim();
                    if (!string.IsNullOrEmpty(headerContent)) textBlocks.Add(TextBlock.Header(headerContent, TextBlockUtil.CalculateHeaderLevel(headerContent)));
                }
                else if (match.Groups["ulist_header"].Success)
                {
                    string rawHeader = match.Groups["ulist_header"].Value.Trim();
                    if (!string.IsNullOrEmpty(rawHeader))
                    {
                        string cleanHeader = Regex.Replace(rawHeader, @"\*\*(.+?)\*\*", "$1");
                        textBlocks.Add(TextBlock.UListHeader(cleanHeader)); // Assuming level 0 for list headers
                    }
                }
                else if (match.Groups["ulist"].Success)
                {
                    string listContent = match.Groups["ulist"].Value.Trim();
                    if (!string.IsNullOrEmpty(listContent)) textBlocks.Add(TextBlock.UList(TextBlockUtil.ProcessTags(listContent)));
                }
                else if (match.Groups["code"].Success)
                {
                    string language = match.Groups["language"].Value;
                    string code = match.Groups["code"].Value.Trim();
                    if (!string.IsNullOrEmpty(code)) textBlocks.Add(TextBlock.CodeBlock(language, code));
                }
                else if (match.Groups["quote"].Success)
                {
                    string quoteContent = match.Groups["quote"].Value.Trim();
                    if (!string.IsNullOrEmpty(quoteContent)) textBlocks.Add(TextBlock.Quote(TextBlockUtil.ProcessTags(quoteContent)));
                }

                lastIndex = match.Index + match.Length;
            }

            // Add any remaining plain text after the last match
            if (lastIndex < text.Length)
            {
                string plainText = text.Substring(lastIndex).Trim();
                if (!string.IsNullOrEmpty(plainText)) textBlocks.Add(TextBlock.Text(plainText));
            }

            _cachedTextBlocks.Add(hashCode, textBlocks);
            DrawBlocks(textBlocks, maxWidth);
        }
    }
}