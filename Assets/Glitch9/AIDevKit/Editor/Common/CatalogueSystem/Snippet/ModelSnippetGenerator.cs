using System.Collections.Generic;
using Glitch9.Editor.CodeGen;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    internal static class ModelSnippetGenerator
    {
        internal static void Generate(Api api)
        {
            List<Model> models = ModelLibrary.GetModelsByAPI(api);
            if (models.IsNullOrEmpty()) throw new System.Exception("No models to generate.");

            Debug.Log($"Generating model snippets for {api}...");

            string className = AssetSnippetUtil.ResolveModelClassName(api);
            string namespaceName = AssetSnippetUtil.ResolveNamespace(api);
            string targetDir = AIDevKitEditorPath.FindConfigFilePath(api);
            string writePath = System.IO.Path.Combine(targetDir, $"{className}.cs");

            CodeGenBuilder builder = new();

            builder.AddDirectiveComment(DirectiveComment.ReSharperDisableAll);
            builder.SetNamespace(namespaceName);
            builder.AddClass(className);

            //models.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));
            models.Sort((x, y) => x.Capability.CompareTo(y.Capability));

            HashSet<string> dupeChecks = new();
            bool deprecatedExists = false;

            foreach (Model model in models)
            {
                if (model == null)
                {
                    Debug.LogWarning($"{typeof(Model).Name} is null. Skipping...");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    Debug.LogWarning($"{typeof(Model).Name} ID is null or empty. Skipping...");
                    continue;
                }

                //if (model.Model.IsFineTuned) continue;
                //if (model.Model.IsDeprecated) continue;

                string propertyName = AssetSnippetUtil.ResolveModelPropertyName(model.Id);
                string value = model.Id;

                if (dupeChecks.Contains(value))
                {
                    Debug.LogWarning($"Duplicate {typeof(Model).Name} name '{value}' found. Skipping...");
                    continue;
                }

                dupeChecks.Add(value);

                List<CodeGenComment> comments = new();
                List<CodeGenAttribute> attributes = new();

                //List<string> commentLines = new();
                //if (!string.IsNullOrEmpty(model.Description)) commentLines.Add(model.Description); 
                //if (commentLines.Count > 0) comments.Add(CodeGenComment.Summary(commentLines.ToArray()));

                if (model.MaxOutputTokens != 0)
                {
                    // (e.g,) Returns a maximum of 128,000 tokens.
                    comments.Add(CodeGenComment.Remarks($"Returns a maximum of {model.MaxOutputTokens} tokens."));
                }

                if (model.MaxInputTokens != 0)
                {
                    // (e.g,) A maximum of 4096 tokens can be processed at a time.
                    comments.Add(CodeGenComment.Remarks($"A maximum of {model.MaxInputTokens} tokens can be processed at a time."));
                }

                // string inspectorName = ModelNameRegex.ResolveInspectorName(model.Name);
                // attributes.Add(CodeGenAttribute.Create("ApiEnum", $"\"{inspectorName}\"", $"\"{model.Id}\""));

                if (model.IsDeprecated)
                {
                    string message = $"This {typeof(Model).Name} is deprecated.";
                    attributes.Add(CodeGenAttribute.Obsolete($"\"{message}\""));
                    deprecatedExists = true;
                }

                builder.AddContString(className, propertyName, value, AccessModifier.Public, comments, attributes);
            }

            if (deprecatedExists) builder.AddUsing("System"); // to use Obsolete attribute

            builder.Generate(writePath);
        }
    }
}