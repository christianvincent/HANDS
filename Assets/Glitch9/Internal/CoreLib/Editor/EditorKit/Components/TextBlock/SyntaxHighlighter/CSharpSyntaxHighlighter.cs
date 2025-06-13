using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Glitch9.Editor
{
    public class CSharpSyntaxHighlighter : SyntaxHighlighter
    {
        private static readonly string[] blueKeywords =
        {
            "using", "namespace", "class", "public", "private", "protected", "static", "override",
            "void", "int", "string", "true", "false", "null", "new", "finally", "in", "var", "bool",
            "float", "double", "long", "short", "decimal", "byte", "sbyte", "char", "object",
            "dynamic", "async", "await", "this", "base", "get", "set", "add", "remove", "lock",
            "volatile", "readonly", "sealed", "abstract", "virtual", "interface", "enum", "struct",
            "delegate", "event", "checked", "unchecked", "fixed", "unsafe", "is", "as", "sizeof",
            "typeof", "default", "nameof", "params", "ref", "out", "in", "where", "yield",
            "internal", "extern", "partial", "dynamic", "using static", "using alias",
        };

        private static readonly string[] pinkKeywords =
        {
            "return", "if", "else", "while", "for", "foreach", "break", "continue",
            "switch", "case", "default", "try", "catch", "throw"
        };

        private static readonly string[] greenKeywords =
        {
            "Task", "IEnumerable", "IEnumerator", "IDisposable", "Action", "Func",
            "List", "Dictionary", "HashSet", "Queue", "Stack", "Array", "StringBuilder",
            "String", "DateTime", "TimeSpan", "Uri", "Regex", "JsonSerializer",
            "UniTask", "ValueTask", "CancellationToken",
        };

        protected override string HighlightInternal(string code)
        {
            // 1. Extract and mask comments first
            var commentMatches = Regex.Matches(code, @"//.*");
            var commentMap = new Dictionary<string, string>();
            for (int i = 0; i < commentMatches.Count; i++)
            {
                string key = $"§COMMENT§{i}";
                commentMap[key] = $"<color={Colors.Gray}>{commentMatches[i].Value}</color>";
                code = code.Replace(commentMatches[i].Value, key);
            }

            // 2. Extract and mask strings first
            var stringMatches = Regex.Matches(code, "@\"(?:[^\"]|\"\")*\"|\"(?:\\\\.|[^\"\\\\])*\"");
            var stringMap = new Dictionary<string, string>();
            for (int i = 0; i < stringMatches.Count; i++)
            {
                string key = $"§STRING§{i}§";
                stringMap[key] = $"<color={Colors.Orange}>{stringMatches[i].Value}</color>";
                code = code.Replace(stringMatches[i].Value, key);
            }
            var userTypes = Regex.Matches(code, @"\b(class|struct|interface|enum)\s+(\w+)")
              .Cast<Match>()
              .Select(m => m.Groups[2].Value)
              .ToHashSet();

            // 3. Highlight strings (double-quoted)
            code = Regex.Replace(code, "\".*?\"", m => $"<color={Colors.Orange}>{m.Value}</color>");

            // 4. Highlight class or object instantiations
            code = Regex.Replace(code, @"\b(class|new)\s+(\w+)\b", m =>
                $"{m.Groups[1].Value} <color={Colors.Green}>{m.Groups[2].Value}</color>");

            // 4.5. Highlight everything after 'namespace' keyword. Even after dots.
            code = Regex.Replace(code, @"\bnamespace\s+(\w+(?:\.\w+)*)", m =>
            {
                string ns = m.Groups[1].Value;
                return $"namespace <color={Colors.Green}>{ns}</color>";
            });

            // 5. Highlight keywords  
            code = RegexReplaceWordList(code, blueKeywords, Colors.Blue);
            code = RegexReplaceWordList(code, pinkKeywords, Colors.Pink);
            code = RegexReplaceWordList(code, greenKeywords, Colors.Green);

            // 6. Highlight method names
            code = Regex.Replace(code, @"\b(\w+)(?=\()", m =>
                $"<color={Colors.Red}>{m.Groups[1].Value}</color>");

            // 7. Highlight user-defined types elsewhere
            foreach (string type in userTypes)
            {
                code = Regex.Replace(code, $@"\b{type}\b", $"<color={Colors.Green}>{type}</color>");
            }

            // 7.5. Highlight generics (<word>) <> is white and word is green
            code = Regex.Replace(code, @"<(\w+)>", m =>
                $"<color={Colors.White}><color={Colors.Green}>{m.Groups[1].Value}</color></color>");

            // 마지막에 복원
            foreach (var kvp in stringMap)
                code = code.Replace(kvp.Key, kvp.Value);

            foreach (var kvp in commentMap)
                code = code.Replace(kvp.Key, kvp.Value);

            return code;
        }

        private string RegexReplaceWordList(string input, IEnumerable<string> keywords, string color)
        {
            string pattern = $@"\b({string.Join("|", keywords)})\b";
            return Regex.Replace(input, pattern, m => $"<color={color}>{m.Value}</color>");
        }
    }
}
