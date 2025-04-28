using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Fwk.Addressables;

public class SingleImageLoadTest : MonoBehaviour
{
    [SerializeField] private Button loadImageButton;
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private Transform imageParent;
    [SerializeField] private string imageKey;
    AddressableCache addressableCache = new();

    private void Start()
    {
        loadImageButton.onClick.AddListener(OnLoadImageButtonClicked);
    }

    private void OnDestroy()
    {
        addressableCache.Dispose();
    }

    private void OnLoadImageButtonClicked()
    {
        LoadImageAsync(imageKey).Forget();
    }

    private async UniTask LoadImageAsync(string key)
    {
        var image = await addressableCache.LoadAsync<Sprite>(key);
        if (image != null)
        {
            var imageObject = Instantiate(imagePrefab, imageParent);
            imageObject.SetActive(true);
            var imageComponent = imageObject.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.sprite = image;
            }
        }
        else
        {
            Debug.LogError($"Failed to load image with key: {key}");
        }
    }
}