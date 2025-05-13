using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Fwk
{
    public class CameraManager : SingletonPersistent<CameraManager>
    {
        public Camera MainCamera { get; private set; }
        public Camera UICamera { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            UpdateCameras();
        }

        public static void CreateIfNotExists()
        {
            if (Instance != null)
            {
                return;
            }
            var go = new GameObject("CameraManager");
            Instance = go.AddComponent<CameraManager>();
            go.AddComponent<AudioListener>();
        }

        public void UpdateCameras()
        {
            FindAndSetMainCamera();
            FindAndSetUICamera();
            SetCanvasCameras();
            DestroyOtherCameras();
            SetCameraStack();
        }

        private void SetCameraStack()
        {
            if (MainCamera != null && UICamera != null)
            {
                var mainCameraStack = MainCamera.GetUniversalAdditionalCameraData();
                if (!mainCameraStack.cameraStack.Contains(UICamera))
                {
                    mainCameraStack.cameraStack.Add(UICamera);
                }
            }
        }

        private void SetCanvasCameras()
        {
            var canvasCameras = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvasCameras)
            {
                if (canvas.worldCamera == null)
                {
                    continue;
                }
                if (canvas.worldCamera.gameObject.CompareTag("MainCamera"))
                {
                    canvas.worldCamera = MainCamera;
                }
                else
                {
                    canvas.worldCamera = UICamera;
                }
            }
        }
        private void FindAndSetMainCamera()
        {
            if (MainCamera != null)
            {
                return;
            }
            var mainCameraObject = GameObject.FindWithTag("MainCamera");
            if (mainCameraObject != null)
            {
                MainCamera = mainCameraObject.GetComponent<Camera>();
                MainCamera.transform.SetParent(transform, false);
            }
        }

        private void FindAndSetUICamera()
        {
            if (UICamera != null)
            {
                return;
            }
            var uiCameraObject = GameObject.FindWithTag("UICamera");
            if (uiCameraObject != null)
            {
                UICamera = uiCameraObject.GetComponent<Camera>();
                UICamera.transform.SetParent(transform, false);
            }
        }

        private void DestroyOtherCameras()
        {
            var foundCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var camera in foundCameras)
            {
                if (camera != MainCamera && camera != UICamera)
                {
                    Destroy(camera.gameObject);
                }
            }
        }
    }
}