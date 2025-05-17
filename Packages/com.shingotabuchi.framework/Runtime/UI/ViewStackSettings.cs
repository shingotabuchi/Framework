using UnityEngine;
using UnityEngine.UI;

namespace Fwk.UI
{
    public struct ViewStackSettings
    {
        public readonly RenderMode RenderMode;
        public readonly string SortingLayerName;
        public readonly CanvasScaler.ScaleMode ScaleMode;
        public readonly CanvasScaler.ScreenMatchMode ScreenMatchMode;
        public readonly Vector2 ReferenceResolution;
        public readonly int SortingOrder;
        public readonly float PlaneDistance;
        public readonly float MatchWidthOrHeight;
        public readonly bool VertexColorAlwaysGammaSpace;

        public ViewStackSettings(
            RenderMode renderMode,
            string sortingLayerName,
            CanvasScaler.ScaleMode scaleMode,
            CanvasScaler.ScreenMatchMode screenMatchMode,
            Vector2 referenceResolution,
            int sortingOrder,
            float planeDistance,
            float matchWidthOrHeight,
            bool vertexColorAlwaysGammaSpace
        )
        {
            RenderMode = renderMode;
            SortingLayerName = sortingLayerName;
            ScaleMode = scaleMode;
            ScreenMatchMode = screenMatchMode;
            ReferenceResolution = referenceResolution;
            SortingOrder = sortingOrder;
            PlaneDistance = planeDistance;
            MatchWidthOrHeight = matchWidthOrHeight;
            VertexColorAlwaysGammaSpace = vertexColorAlwaysGammaSpace;
        }
    }
}