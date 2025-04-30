using UnityEngine;

namespace Fwk.UI
{
    public static class ShowAndHideUI
    {
        public static void HideUI(this GameObject gameObject)
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
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0f;
        }

        public static void ShowUI(this GameObject gameObject)
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
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
    }
}