using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIRendererFeature : ScriptableRendererFeature
{
    [SerializeField]
    private LayerMask _layerMask;

    [SerializeField]
    private bool _execblur = false;
    [SerializeField]
    private bool _execGlassMorphism = false;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float _blurRate = 1.0f;
    [SerializeField]
    [Range(0.5f, 3.0f)]
    private float _blurWidth = 2.0f;
    [SerializeField]
    [Range(0.1f, 1.0f)]
    private float _blurRenderScale = 0.5f;

    [SerializeField]
    private Shader _blurShader;

    private UIRenderPass _uiRenderPass;

    public override void Create()
    {
        if (_blurShader == null)
        {
            return;
        }

        Material blurMaterial = CoreUtils.CreateEngineMaterial(_blurShader);
        _uiRenderPass = new UIRenderPass(blurMaterial, _layerMask);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_uiRenderPass == null)
        {
            return;
        }

        renderer.EnqueuePass(_uiRenderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        _uiRenderPass?.Setup(_execblur, _execGlassMorphism, _blurRate, _blurWidth, _blurRenderScale);
    }
}