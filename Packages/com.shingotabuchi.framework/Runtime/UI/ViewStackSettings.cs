using UnityEngine;
using UnityEngine.UI;

namespace Fwk.UI
{
    public struct ViewStackSettings
    {
        public readonly RenderMode RenderMode;
        public readonly UnityEngine.UI.CanvasScaler.ScaleMode ScaleMode;
        public readonly CanvasScaler.ScreenMatchMode ScreenMatchMode;
        public readonly Vector2 ReferenceResolution;
        public readonly int SortingOrder;
        public readonly float PlaneDistance;
        public readonly float MatchWidthOrHeight;
        public readonly bool VertexColorAlwaysGammaSpace;

        public ViewStackSettings(
            RenderMode renderMode,
            UnityEngine.UI.CanvasScaler.ScaleMode scaleMode,
            CanvasScaler.ScreenMatchMode screenMatchMode,
            Vector2 referenceResolution,
            int sortingOrder,
            float planeDistance,
            float matchWidthOrHeight,
            bool vertexColorAlwaysGammaSpace
        )
        {
            RenderMode = renderMode;
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