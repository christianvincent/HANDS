using System;
using System.Collections.Generic;

namespace Glitch9.AIDevKit.Editor
{
    public partial class ModelCatalogueWindow
    {
        public class ModelCatalogueContextMenuHandler : TreeViewContextMenuHandler
        {
            public ModelCatalogueContextMenuHandler(ModelCatalogueTreeView treeView) : base(treeView)
            {
            }

            public override IEnumerable<TreeViewAction> CreateContextMenus()
            {
                yield return new TreeViewAction()
                {
                    Name = "Add to Library",
                    Action = AddToLibrary,
                    Condition = CanAddToLibrary,
                };
                yield return new TreeViewAction()
                {
                    Name = "Remove from Library",
                    Action = RemoveFromLibrary,
                    Condition = CanRemoveFromLibrary,
                };
                yield return new TreeViewAction()
                {
                    Name = "Update Metadata",
                    Action = UpdateMetadata,
                };
            }

            private void AddToLibrary(ModelCatalogueTreeViewItem[] items, Action<bool> onSuccess)
            {
                if (items.IsNullOrEmpty())
                {
                    onSuccess?.Invoke(false);
                    return;
                }

                bool successAtLeastOne = false;

                foreach (var item in items)
                {
                    if (item == null || item.IsInvalid())
                    {
                        continue;
                    }

                    if (ModelCatalogueUtil.AddToLibrary(item))
                    {
                        successAtLeastOne = true;
                    }
                }

                onSuccess?.Invoke(successAtLeastOne);
            }

            private bool CanAddToLibrary(ModelCatalogueTreeViewItem[] items)
            {
                if (items.IsNullOrEmpty()) return false;

                foreach (var item in items)
                {
                    if (item == null || item.IsInvalid() || item.InMyLibrary)
                    {
                        continue;
                    }

                    return true;
                }

                return false;
            }

            private void RemoveFromLibrary(ModelCatalogueTreeViewItem[] items, Action<bool> onSuccess)
            {
                if (items.IsNullOrEmpty())
                {
                    onSuccess?.Invoke(false);
                    return;
                }

                bool successAtLeastOne = false;

                foreach (var item in items)
                {
                    if (item == null || item.IsInvalid())
                    {
                        continue;
                    }

                    if (ModelCatalogueUtil.RemoveFromLibrary(item))
                    {
                        successAtLeastOne = true;
                    }
                }

                onSuccess?.Invoke(successAtLeastOne);
            }

            private bool CanRemoveFromLibrary(ModelCatalogueTreeViewItem[] items)
            {
                if (items.IsNullOrEmpty()) return false;

                foreach (var item in items)
                {
                    if (item == null || item.IsInvalid() || !item.InMyLibrary || !item.CanDelete)
                    {
                        continue;
                    }

                    return true;
                }

                return false;
            }

            private async void UpdateMetadata(ModelCatalogueTreeViewItem[] items, Action<bool> onSuccess)
            {
                if (items.IsNullOrEmpty())
                {
                    onSuccess?.Invoke(false);
                    return;
                }

                bool successAtLeastOne = false;

                foreach (var item in items)
                {
                    if (item == null || item.IsInvalid())
                    {
                        continue;
                    }

                    if (await ModelCatalogueUtil.UpdateMetadataAsync(item))
                    {
                        successAtLeastOne = true;
                    }
                }

                onSuccess?.Invoke(successAtLeastOne);
                _treeView.ReloadTreeView(true, true);
            }
        }
    }
}