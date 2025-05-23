using UnityEngine;
using Cysharp.Threading.Tasks;
using Fwk.Addressables;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Fwk.UI
{
    public class ViewStackManager : MonoBehaviour
    {
        private const string _defaultStackName = "Default";
        private const string _defaultAssetLabel = "StackableViews";
        private IAssetRequester _assetRequester;
        private bool _isInitialized = false;
        private bool _isInitializing = false;
        private readonly Dictionary<Type, StackableView> _uiCache = new();
        private readonly Dictionary<string, ViewStack> _stackDict = new();
        private IBlurController _blurController;

        public async UniTask Initialize(
            IAssetRequester assetRequester,
            string assetLabel,
            ViewStackSettings defaultStackSettings,
            IBlurController blurController,
            CancellationToken token)
        {
            while (true)
            {
                if (_isInitialized)
                {
                    return;
                }
                if (_isInitializing)
                {
                    await UniTask.Yield(token);
                    continue;
                }
                _isInitializing = true;
                try
                {
                    _assetRequester = assetRequester;
                    _blurController = blurController;
                    await InitializeInternal(defaultStackSettings, token, assetLabel);
                }
                finally
                {
                    _isInitializing = false;
                }
                break;
            }
        }

        private async UniTask InitializeInternal(
            ViewStackSettings defaultStackSettings,
            CancellationToken token,
            string assetLabel = _defaultAssetLabel)
        {
            if (CameraManager.Instance == null)
            {
                CameraManager.CreateIfNotExists();
            }

            var keys = await AddressableManager.GetKeysByLabel(assetLabel, cancellationToken: token);
            foreach (var key in keys)
            {
                Debug.Log($"Preloding stackable view: {key}");
            }
            await _assetRequester.Preload<GameObject>(keys, token);

            foreach (var key in keys)
            {
                var uiAsset = _assetRequester.GetAssetImmediate<GameObject>(key);
                var ui = Instantiate(uiAsset, transform);
                ui.SetActive(false);
                var view = ui.GetComponent<StackableView>();
                if (view == null)
                {
                    Debug.LogError($"View component not found in {key}");
                    continue;
                }
                var type = view.GetType();
                _uiCache.Add(type, view);
            }

            CreateStack(_defaultStackName, defaultStackSettings);
            _isInitialized = true;
        }

        public void SetOnNewFrontView(Action<StackableView> onNewFrontView, string stackName = _defaultStackName)
        {
            if (!_stackDict.ContainsKey(stackName))
            {
                Debug.LogError($"Stack {stackName} not found.");
                return;
            }
            _stackDict[stackName].SetOnNewFrontView(onNewFrontView);
        }

        public void CreateStack(string stackName, ViewStackSettings settings)
        {
            if (_stackDict.ContainsKey(stackName))
            {
                Debug.Log($"Stack {stackName} already exists.");
                return;
            }
            var stack = new ViewStack(stackName, settings, transform, _blurController);
            _stackDict.Add(stackName, stack);
        }

        public T AddToFront<T>(string stackName = _defaultStackName) where T : StackableView
        {
            if (!_isInitialized)
            {
                Debug.LogError("ViewStackManager is not initialized.");
                return null;
            }
            var view = GetView<T>();
            if (view == null)
            {
                Debug.LogError($"View not found for {typeof(T)}");
                return null;
            }
            var stack = _stackDict[stackName];
            if (stack.Contains(view))
            {
                Debug.Log($"View {view} is already in the stack.");
                return view as T;
            }
            stack.AddToFront(view);
            return view as T;
        }

        public T AddToBack<T>(string stackName = _defaultStackName) where T : StackableView
        {
            if (!_isInitialized)
            {
                Debug.LogError("ViewStackManager is not initialized.");
                return null;
            }
            var view = GetView<T>();
            if (view == null)
            {
                Debug.LogError($"View not found for {typeof(T)}");
                return null;
            }
            var stack = _stackDict[stackName];
            if (stack.Contains(view))
            {
                Debug.Log($"View {view} is already in the stack.");
                return view as T;
            }
            stack.AddToBack(view);
            return view as T;
        }

        public void RemoveFromFront<T>(string stackName = _defaultStackName) where T : StackableView
        {
            if (!_isInitialized)
            {
                Debug.LogError("ViewStackManager is not initialized.");
                return;
            }
            var view = GetView<T>();
            if (view == null)
            {
                Debug.LogError($"View not found for {typeof(T)}");
                return;
            }
            var stack = _stackDict[stackName];
            if (stack.PeekFront() != view)
            {
                Debug.LogError($"View {view} is not at the front of the stack.");
                return;
            }
            stack.RemoveFromFront(view);
        }

        public void RemoveFromBack<T>(string stackName = _defaultStackName) where T : StackableView
        {
            if (!_isInitialized)
            {
                Debug.LogError("ViewStackManager is not initialized.");
                return;
            }
            var view = GetView<T>();
            if (view == null)
            {
                Debug.LogError($"View not found for {typeof(T)}");
                return;
            }
            var stack = _stackDict[stackName];
            if (stack.PeekBack() != view)
            {
                Debug.LogError($"View {view} is not at the back of the stack.");
                return;
            }
            stack.RemoveFromBack(view);
        }

        public T GetView<T>() where T : StackableView
        {
            return _uiCache[typeof(T)] as T;
        }

        public void SetFrontViewBelowBlur(string stackName = _defaultStackName)
        {
            if (!_isInitialized)
            {
                Debug.LogError("ViewStackManager is not initialized.");
                return;
            }
            var stack = _stackDict[stackName];
            stack.SetFrontViewBelowBlur();
        }

        public void SetFrontViewAboveBlur(string stackName = _defaultStackName)
        {
            if (!_isInitialized)
            {
                Debug.LogError("ViewStackManager is not initialized.");
                return;
            }
            var stack = _stackDict[stackName];
            stack.SetFrontViewAboveBlur();
        }
    }
}