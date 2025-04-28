using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Fwk.Addressables;

public class SingleImageLoadTest : MonoBehaviour
{
    [SerializeField] private Button loadButton;
    [SerializeField] private Button releaseButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private Transform imageParent;
    [SerializeField] private string imageKey;
    AddressableCache addressableCache = new();

    private void Start()
    {
        loadButton.onClick.AddListener(OnLoadButtonClicked);
        releaseButton.onClick.AddListener(OnReleaseButtonClicked);
        resetButton.onClick.AddListener(OnResetButtonClicked);
    }

    private void OnDestroy()
    {
        addressableCache.Dispose();
    }

    private void OnLoadButtonClicked()
    {
        LoadAsync(imageKey).Forget();
    }

    private void OnReleaseButtonClicked()
    {
        addressableCache.Release(imageKey);
    }

    private void OnResetButtonClicked()
    {
        addressableCache.Dispose();
        addressableCache = new();
        for (int i = 0; i < imageParent.childCount; i++)
        {
            var childObject = imageParent.GetChild(i).gameObject;
            if (childObject != null && childObject != imagePrefab)
            {
                Destroy(childObject);
            }
        }
    }

    private async UniTask LoadAsync(string key)
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