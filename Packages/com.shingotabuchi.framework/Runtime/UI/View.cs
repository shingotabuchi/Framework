using UnityEngine;

namespace Fwk.UI
{
    public class View : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActiveFast(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActiveFast(false);
        }
    }
}