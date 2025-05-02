using UnityEngine;

namespace Fwk.UI
{
    public static class ShowAndHideUI
    {
        public static void UISetVisible(this GameObject gameObject, bool visible, bool changeInteractable = true)
        {
            if (gameObject == null)
            {
                return;
            }
            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            if (changeInteractable)
            {
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            canvasGroup.alpha = visible ? 1f : 0f;
        }

        public static void UISetInteractable(this GameObject gameObject, bool interactable)
        {
            if (gameObject == null)
            {
                return;
            }
            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }
    }
}