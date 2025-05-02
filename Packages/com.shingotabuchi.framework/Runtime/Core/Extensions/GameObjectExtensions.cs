using UnityEngine;

namespace Fwk
{
    public static class GameObjectExtensions
    {
        public static void SetActiveFast(this GameObject self, bool active)
        {
            if (self.activeSelf != active)
            {
                self.gameObject.SetActive(active);
            }
        }
    }
}