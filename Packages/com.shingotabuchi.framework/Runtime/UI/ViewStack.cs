using System;
using UnityEngine;
using UnityEngine.UI;

namespace Fwk.UI
{
    public class ViewStack
    {
        private readonly Canvas _canvas;
        private readonly Deque<StackableView> _stack = new();
        private Action<StackableView> _onNewFrontView;


        public ViewStack(string name, ViewStackSettings settings, Transform parent)
        {
            _canvas = CreateCanvas(name, settings, parent);
        }

        private Canvas CreateCanvas(string name, ViewStackSettings settings, Transform parent)
        {
            var canvas = new GameObject(name + "ViewStackCanvas").AddComponent<Canvas>();
            var canvasScaler = canvas.gameObject.AddComponent<CanvasScaler>();
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            canvas.gameObject.layer = LayerMask.NameToLayer("UI");
            canvas.transform.SetParent(parent, false);
            canvas.renderMode = settings.RenderMode;
            canvas.worldCamera = CameraManager.Instance.UICamera;
            canvas.sortingOrder = settings.SortingOrder;
            canvas.planeDistance = settings.PlaneDistance;
            canvas.vertexColorAlwaysGammaSpace = settings.VertexColorAlwaysGammaSpace;
            canvasScaler.matchWidthOrHeight = settings.MatchWidthOrHeight;
            canvasScaler.uiScaleMode = settings.ScaleMode;
            canvasScaler.screenMatchMode = settings.ScreenMatchMode;
            canvasScaler.referenceResolution = settings.ReferenceResolution;
            return canvas;
        }

        public void SetOnNewFrontView(Action<StackableView> onNewFrontView)
        {
            _onNewFrontView = onNewFrontView;
        }

        public void AddToFront(StackableView view)
        {
            if (_stack.Contains(view))
            {
                Debug.Log($"View {view} is already in the stack.");
                return;
            }

            if (_stack.Count > 0)
            {
                var lastView = _stack.PeekBack();
                lastView.OnCovered(view);
            }

            _stack.AddToFront(view);
            Debug.Log($"Added {view} to front of stack.");

            OnNewFrontView(view);
            view.transform.SetParent(_canvas.transform, false);
        }

        public void AddToBack(StackableView view)
        {
            if (_stack.Contains(view))
            {
                Debug.Log($"View {view} is already in the stack.");
                return;
            }
            _stack.AddToBack(view);
            Debug.Log($"Added {view} to back of stack.");

            if (_stack.Count == 1)
            {
                OnNewFrontView(view);
            }

            view.transform.SetParent(_canvas.transform, false);
        }

        public void RemoveFromFront(StackableView view)
        {
            if (_stack.PeekFront() != view)
            {
                Debug.LogError($"{view} is not at the front of the stack.");
                return;
            }

            var stackView = _stack.RemoveFromFront();
            Debug.Log($"Removed {stackView} from front of stack.");
            stackView.OnRemoveFromFront();

            if (_stack.Count > 0)
            {
                var nextView = _stack.PeekFront();
                OnNewFrontView(nextView);
            }
        }

        public void RemoveFromBack(StackableView view)
        {
            if (_stack.PeekBack() != view)
            {
                Debug.LogError($"{view} is not at the back of the stack.");
                return;
            }

            var stackView = _stack.RemoveFromBack();
            Debug.Log($"Removed {stackView} from back of stack.");
            stackView.OnRemoveFromBack();
        }

        public void RemoveFromFront()
        {
            _stack.RemoveFromFront();

            if (_stack.Count > 0)
            {
                var nextView = _stack.PeekFront();
                OnNewFrontView(nextView);
            }
        }

        private void OnNewFrontView(StackableView view)
        {
            _onNewFrontView?.Invoke(view);
            view.OnFront();
        }

        public void RemoveFromBack()
        {
            _stack.RemoveFromBack();
        }

        public bool Contains(StackableView view)
        {
            return _stack.Contains(view);
        }

        public StackableView PeekFront()
        {
            return _stack.PeekFront();
        }

        public StackableView PeekBack()
        {
            return _stack.PeekBack();
        }
    }
}