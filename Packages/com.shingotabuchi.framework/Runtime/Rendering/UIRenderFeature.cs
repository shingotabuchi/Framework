// ref. https://media.colorfulpalette.co.jp/n/nffc0ece136be

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private LayerMask _layerMask;

#if UNITY_EDITOR
    [SerializeField] private bool _debugForceChange;
    [SerializeField] private bool _debugExecBlur;
    [SerializeField] private bool _debugExecGlassMorphism;
    [SerializeField][Range(0.0f, 1.0f)] private float _debugBlurRate = 1.0f;
#endif

    [SerializeField][Range(0.5f, 3.0f)] private float _blurWidth = 1.2f;
    [SerializeField][Range(0.1f, 1.0f)] private float _blurRenderScale = 0.5f;

    [SerializeField] private Shader _blurShader;

    private UIRenderPass _uiRenderPass;

    private readonly UIRendererFeatureParameter _parameter = new();

    public static UIRendererFeature Instance { get; private set; }

    public static bool ExistsInstance()
    {
        return Instance != null;
    }

    public override void Create()
    {
        if (_blurShader == null)
        {
            return;
        }

        var blurMaterial = CoreUtils.CreateEngineMaterial(_blurShader);
        _uiRenderPass = new UIRenderPass(blurMaterial, _layerMask);
        Instance = this;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game)
        {
            return;
        }
        if (_uiRenderPass == null)
        {
            return;
        }

        renderer.EnqueuePass(_uiRenderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var execBlur = _parameter.ExecBlur;
        var execGlassMorphism = _parameter.ExecGlassMorphism;
        var blurRate = _parameter.BlurRate;
#if UNITY_EDITOR
        if (_debugForceChange)
        {
            execBlur = _debugExecBlur;
            execGlassMorphism = _debugExecGlassMorphism;
            blurRate = _debugBlurRate;
        }
#endif
        _uiRenderPass?.Setup(execBlur, execGlassMorphism, blurRate, _blurWidth, _blurRenderScale);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            StopBlur();
            StopGlassMorphism();
            _uiRenderPass?.Dispose();
            Instance = null;
        }
    }

    public bool IsExecBlur()
    {
        return _parameter.ExecBlur;
    }

    public void ExecBlur(float rate = 1f)
    {
        _parameter.ExecBlur = true;
        ChangeBlurRate(rate);
    }

    public void StopBlur(float rate = 0f)
    {
        _parameter.ExecBlur = false;
        ChangeBlurRate(rate);
    }

    public bool IsExecGlassMorphism()
    {
        return _parameter.ExecGlassMorphism;
    }

    public void ExecGlassMorphism(float rate = 1f)
    {
        _parameter.ExecGlassMorphism = true;
        ChangeBlurRate(rate);
    }

    public void StopGlassMorphism(float rate = 0f)
    {
        _parameter.ExecGlassMorphism = false;
        ChangeBlurRate(rate);
    }

    public void ChangeBlurRate(float rate)
    {
        _parameter.BlurRate = rate;
    }
}
