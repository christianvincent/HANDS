using Glitch9.Editor;
using Glitch9.Editor.IMGUI;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Glitch9.AIDevKit.Editor
{
    public partial class ModelCatalogueWindow
    {
        public class ModelCatalogueTreeView : ExtendedTreeView
        {
            public ModelCatalogueTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader) : base(treeViewState, multiColumnHeader)
            {
            }

            protected override IEnumerable<ModelCatalogueEntry> GetSourceData()
            {
                return ModelCatalogue.Instance.Entries;
            }

            protected override void CellGUI(Rect cellRect, TreeViewItem item, int columnIndex, ref RowGUIArgs args)
            {
                if (item is not ModelCatalogueTreeViewItem i) return;

                switch (columnIndex)
                {
                    case ColumnIndex.IN_LIB:
                        TreeViewGUI.CheckCircleCell(cellRect, i.InMyLibrary);
                        break;

                    case ColumnIndex.API:
                        AIDevKitGUI.TreeView.ApiColumn(cellRect, i.Api);
                        break;

                    // case ColumnIndex.PROVIDER:
                    //     TreeViewGUI.StringCell(cellRect, i.ModelProvider);
                    //     break;

                    case ColumnIndex.NAME:
                        Rect contentRect = cellRect;
                        if (i.IsNew)
                        {
                            Rect[] rectSplit = contentRect.SplitHorizontallyFixed(22f);
                            GUI.Label(rectSplit[0], new GUIContent(EditorIcons.NewBadge));
                            contentRect = rectSplit[1];
                        }
                        TreeViewGUI.ContentCell(contentRect, new GUIContent(i.Name, i.Description));
                        break;

                    case ColumnIndex.FAMILY:
                        TreeViewGUI.StringCell(cellRect, i.FamilyDisplayName);
                        break;

                    // case ColumnIndex.FAMILY_VERSION:
                    //     TreeViewGUI.StringCell(cellRect, i.FamilyVersion);
                    //     break;

                    case ColumnIndex.CAPABILITY:
                        AIDevKitGUI.TreeView.FeaturesColumn(cellRect, i.Capability);
                        break;

                    case ColumnIndex.PER_INPUT_TOKEN:
                        AIDevKitGUI.TreeView.TokenCostColumn(cellRect, i.Per1MInputToken);
                        break;

                    case ColumnIndex.PER_OUTPUT_TOKEN:
                        AIDevKitGUI.TreeView.TokenCostColumn(cellRect, i.Per1MOutputToken);
                        break;

                    case ColumnIndex.CREATED:
                        TreeViewGUI.UnixDateCell(cellRect, i.CreatedAt);
                        break;
                }
            }
        }
    }
}
