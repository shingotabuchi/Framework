using System.Collections.Generic;
using UnityEngine;

namespace Fwk.UI
{
    public class View : MonoBehaviour
    {
        private List<View> _childViews = new List<View>();

        protected virtual void Awake()
        {
            GetChildViews();
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
                childView.Show();
            }
        }

        public virtual void Hide()
        {
            gameObject.SetActiveFast(false);
            foreach (var childView in _childViews)
            {
                childView.Hide();
            }
        }

        public virtual void OnParentShow()
        {
        }

        public virtual void OnParentHide()
        {
        }
    }
}