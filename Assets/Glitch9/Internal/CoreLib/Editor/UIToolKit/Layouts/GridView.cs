using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Glitch9.Editor.UIToolKit
{
    public class GridView<T> : VisualElement
    {
        private static readonly float[] kImageSizesPerRow =
        {
            80f, 120f, 160f, 200f, 240f, 280f,
            320f, 360f, 400f, 440f, 480f, 520f,
            560f, 600f
        };

        private readonly Func<int, VisualElement> _makeItem;
        private readonly Action<VisualElement, int> _bindItem;
        private int _itemsPerRow = 3; // Default number of items per row
        private float _aspectRatio;
        private readonly float _spacing;
        private List<T> _items = new();

        public GridView(Func<int, VisualElement> makeItem, Action<VisualElement, int> bindItem, float spacing = 8f, float aspectRatio = 1f)
        {
            _makeItem = makeItem ?? throw new ArgumentNullException(nameof(makeItem));
            _bindItem = bindItem ?? throw new ArgumentNullException(nameof(bindItem));
            _spacing = spacing;
            _aspectRatio = aspectRatio;

            /*.preview-container {
                flex-direction: row;
                flex-wrap: wrap;
                justify-content: flex-start;  
            */

            style.flexDirection = FlexDirection.Row;
            style.flexWrap = Wrap.Wrap;
            style.justifyContent = Justify.FlexStart;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Rebuild();
        }

        public void SetItems(List<T> items)
        {
            // Debug.Log($"GridView SetItems called with {items?.Count ?? 0} items.");
            _items.Clear();
            foreach (var item in items)
                _items.Add(item);
            Rebuild();
        }

        public void SetAspectRatio(float aspectRatio)
        {
            // Debug.Log($"GridView SetAspectRatio called with {aspectRatio}.");
            _aspectRatio = aspectRatio;
            Rebuild();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //Debug.Log($"GridView GeometryChangedEvent triggered. New width: {resolvedStyle.width}");
            Rebuild();
        }

        private void Rebuild()
        {
            //Debug.LogWarning("GridView Rebuild called.");
            Clear();

            if (_items == null || _items.Count == 0)
                return;

            float containerWidth = resolvedStyle.width;

            // 🚫 아직 스타일이 반영되지 않아 너비가 0인 경우
            if (containerWidth <= 0 || float.IsNaN(containerWidth))
            {
                //Debug.LogWarning("GridView Rebuild skipped due to invalid container width.");
                return;
            }

            if (_itemsPerRow == 0) _itemsPerRow = 3;

            int newImagesInRow = _itemsPerRow;
            float imageWidth = (containerWidth - _spacing * (_itemsPerRow - 1)) / _itemsPerRow;

            // 🚫 NaN 방지
            if (float.IsNaN(imageWidth) || imageWidth <= 0)
            {
                //Debug.LogWarning("GridView Rebuild skipped due to invalid image width.");
                return;
            }

            float minSize = kImageSizesPerRow[Mathf.Max(0, _itemsPerRow - 1)];
            float maxSize = kImageSizesPerRow[Mathf.Min(_itemsPerRow, kImageSizesPerRow.Length - 1)];

            if (imageWidth > maxSize)
            {
                newImagesInRow = Mathf.Max(1, Mathf.FloorToInt((containerWidth + _spacing) / (maxSize + _spacing)));
            }
            else if (imageWidth < minSize)
            {
                newImagesInRow = Mathf.Min(kImageSizesPerRow.Length - 1, Mathf.FloorToInt((containerWidth + _spacing) / (minSize + _spacing)));
            }

            if (newImagesInRow != _itemsPerRow)
            {
                _itemsPerRow = newImagesInRow;
                imageWidth = (containerWidth - _spacing * (_itemsPerRow - 1)) / _itemsPerRow;
                if (imageWidth <= 0) return;
            }

            float imageHeight = imageWidth / _aspectRatio;

            for (int i = 0; i < _items.Count; i++)
            {
                //Debug.LogWarning($"ItemsPerRow: {_itemsPerRow}, ImageWidth: {imageWidth}, ImageHeight: {imageHeight}");

                if (i % _itemsPerRow == 0)
                {
                    Add(new VisualElement
                    {
                        style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.NoWrap }
                    });
                }

                var itemElement = _makeItem(i);
                _bindItem(itemElement, i);

                itemElement.style.width = imageWidth;
                itemElement.style.height = imageHeight;
                itemElement.style.marginRight = i % _itemsPerRow == _itemsPerRow - 1 ? 0 : _spacing;
                //itemElement.AddToClassList("grid-item");

                var lastRow = this.ElementAt(this.childCount - 1);
                lastRow.Add(itemElement);
            }
        }
    }
}
