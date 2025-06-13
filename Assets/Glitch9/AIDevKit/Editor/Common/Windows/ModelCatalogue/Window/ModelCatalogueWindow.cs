using Glitch9.Editor.IMGUI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    public partial class ModelCatalogueWindow : ExtendedTreeViewWindow
        <
            ModelCatalogueWindow,
            ModelCatalogueWindow.ModelCatalogueTreeView,
            ModelCatalogueTreeViewItem,
            ModelCatalogueWindow.ModelCatalogueDetailsWindow,
            ModelCatalogueEntry,
            ModelCatalogueFilter,
            ModelCatalogueWindow.ModelCatalogueContextMenuHandler
        >
    {
        internal static class ColumnIndex
        {
            public const int IN_LIB = 0;
            public const int API = 1;
            //public const int PROVIDER = 2;
            public const int NAME = 2;
            public const int FAMILY = 3;
            //public const int FAMILY_VERSION = 5;
            public const int CAPABILITY = 4;
            public const int PER_INPUT_TOKEN = 5;
            public const int PER_OUTPUT_TOKEN = 6;
            public const int CREATED = 7;
        }

        [MenuItem(AIDevKitEditor.Paths.ModelCatalogue, priority = AIDevKitEditor.Priorities.ModelCatalogue)]
        public static void ShowWindow() => InitializeWindow(AIDevKitEditor.Labels.ModelCatalogue);

        protected override List<TreeViewColumnData> CreateColumns()
        {
            List<TreeViewColumnData> columns = new()
            {
                new TreeViewColumnData
                {
                    Index = ColumnIndex.IN_LIB,
                    HeaderContent = new GUIContent("✓", "Whether the model is in my library"),
                    FixedWidth = 24f,
                    AutoResize = false,
                },

                new TreeViewColumnData
                {
                    Index = ColumnIndex.API,
                    HeaderContent = new GUIContent("API", "The API that the model belongs to"),
                    Width = TreeViewColumnWidth.Small,
                },

                // new TreeViewColumnData
                // {
                //     Index = ColumnIndex.PROVIDER,
                //     HeaderContent = new GUIContent("Provider"),
                //     Width = TreeViewColumnWidth.Small,
                // },

                new TreeViewColumnData
                {
                    Index = ColumnIndex.NAME,
                    HeaderContent = new GUIContent("Name", "The name of the model"),
                    Width = TreeViewColumnWidth.ExtraWide,
                },

                new TreeViewColumnData
                {
                    Index = ColumnIndex.FAMILY,
                    HeaderContent = new GUIContent("Family", "The model family this model belongs to"),
                    Width = TreeViewColumnWidth.Wide,
                },

                // new TreeViewColumnData
                // {
                //     Index = ColumnIndex.FAMILY_VERSION,
                //     HeaderContent = new GUIContent("Version"),
                // },

                new TreeViewColumnData
                {
                    Index = ColumnIndex.CAPABILITY,
                    HeaderContent = new GUIContent("Features", "Model capabilities"),
                },

                new TreeViewColumnData
                {
                    Index = ColumnIndex.PER_INPUT_TOKEN,
                    HeaderContent = new GUIContent("Input Cost", "Cost per 1M input tokens"),
                    Width = TreeViewColumnWidth.Small,
                },

                new TreeViewColumnData
                {
                    Index = ColumnIndex.PER_OUTPUT_TOKEN,
                    HeaderContent = new GUIContent("Output Cost", "Cost per 1M output tokens"),
                    Width = TreeViewColumnWidth.Small,
                },

                new TreeViewColumnData
                {
                    Index = ColumnIndex.CREATED,
                    HeaderContent = new GUIContent("Created", "The date the model was created"),
                },
            };

            return columns;
        }
    }
}