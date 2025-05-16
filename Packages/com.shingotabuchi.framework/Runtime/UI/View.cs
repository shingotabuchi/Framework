using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

namespace Fwk.UI
{
    public class View : MonoBehaviour
    {
        private readonly List<View> _childViews = new();
        private CancellationTokenSource _hideCancellationTokenSource = new();

        protected virtual void Awake()
        {
            GetChildViews();
        }

        public void CancelHide()
        {
            _hideCancellationTokenSource.Cancel();
            _hideCancellationTokenSource.Dispose();
            _hideCancellationTokenSource = new();
        }

        protected void GetChildViews()
        {
            _childViews.Clear();
            _childViews.AddRange(GetComponentsInChildren<View>(true));
            _childViews.Remove(this);
        }

        public virtual void Show()
        {
            gameObject.SetActiveFast(true);
            foreach (var childView in _childViews)
            {
                childView.OnParentShow();
            }
        }

        public virtual void Hide()
        {
            gameObject.SetActiveFast(false);
            foreach (var childView in _childViews)
            {
                childView.OnParentHide();
            }
        }

        public virtual void OnParentShow()
        {
        }

        public virtual void OnParentHide()
        {
        }

        public async UniTask HideWithDelay(float delay)
        {
            CancelHide();
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _hideCancellationTokenSource.Token);
            Hide();
        }
    }
}