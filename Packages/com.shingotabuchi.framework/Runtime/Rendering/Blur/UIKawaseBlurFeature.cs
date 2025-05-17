using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIKawaseBlurFeature : ScriptableRendererFeature
{
    [SerializeField] Shader _blurShader;
    [SerializeField, Range(1, 4)] int _downSample = 2;
    [SerializeField, Range(1, 5)] int _iterations = 3;

    Material _mat;
    BlurPass _pass;

    public override void Create()
    {
        _mat = CoreUtils.CreateEngineMaterial(_blurShader);
        _pass = new BlurPass(_mat, _downSample, _iterations)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game)
            return;
        renderer.EnqueuePass(_pass);          // no extra setup call needed
    }

    // ────────────────────────────────────────────────────────────────
    class BlurPass : ScriptableRenderPass
    {
        static readonly int BlurTex = Shader.PropertyToID("_BlurTex");

        readonly Material _mat;
        readonly int _downSample, _iterations;

        RenderTargetIdentifier _src;
        int _rtA, _rtB;

        public BlurPass(Material mat, int downSample, int iterations)
        {
            _mat = mat;
            _downSample = downSample;
            _iterations = iterations;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData data)
        {
#if UNITY_2022_2_OR_NEWER      // URP 14+
            _src = data.cameraData.renderer.cameraColorTargetHandle;
#else                           // URP ≤13
            _src = data.renderer.cameraColorTarget;
#endif
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor desc)
        {
            desc.depthBufferBits = 0;
            desc.width >>= _downSample;
            desc.height >>= _downSample;

            _rtA = Shader.PropertyToID("_UIBlurA");
            _rtB = Shader.PropertyToID("_UIBlurB");

            cmd.GetTemporaryRT(_rtA, desc, FilterMode.Bilinear);
            cmd.GetTemporaryRT(_rtB, desc, FilterMode.Bilinear);
        }

        public override void Execute(ScriptableRenderContext ctx, ref RenderingData data)
        {
            var cmd = CommandBufferPool.Get("UI Kawase Blur");

            // copy camera → A  (with down-sample shader pass 0)
            cmd.Blit(_src, _rtA, _mat, 0);

            // ping-pong blur passes (shader pass 1)
            for (int i = 0; i < _iterations; ++i)
            {
                cmd.Blit(_rtA, _rtB, _mat, 1);
                (_rtA, _rtB) = (_rtB, _rtA);   // swap IDs
            }

            // cmd.Blit(_rtA, _src, null, 0);
            cmd.SetGlobalTexture(BlurTex, _rtA);

            ctx.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (_rtA != 0) cmd.ReleaseTemporaryRT(_rtA);
            if (_rtB != 0) cmd.ReleaseTemporaryRT(_rtB);
        }
    }
}
