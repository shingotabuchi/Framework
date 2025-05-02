using Cysharp.Threading.Tasks;
using Fwk.Addressables;
using UnityEngine;

public class LabelLoadTest : MonoBehaviour
{
    private void Start()
    {
        TestLoad().Forget();
    }

    private async UniTask TestLoad()
    {
        var handle = await AddressableManager.LoadByLabelAsync<Sprite>("Test");
        if (handle.Succeeded)
        {
            foreach (var sprite in handle.Objects)
            {
                Debug.Log($"Loaded sprite: {sprite.name}");
            }
        }
        else
        {
            Debug.LogError("Failed to load sprites by label.");
        }
    }
}
