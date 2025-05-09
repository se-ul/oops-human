using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullscreenFogFeature : ScriptableRendererFeature
{
    class FullscreenFogPass : ScriptableRenderPass
    {
        private Material fogMaterial;
        private RTHandle cameraColorTarget;

        public FullscreenFogPass(Material material)
        {
            fogMaterial = material;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (fogMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("FullscreenFog");

            // 직접 카메라 대상 위에 덮어씀 (Blit)
            Blitter.BlitCameraTexture(cmd, cameraColorTarget, cameraColorTarget, fogMaterial, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // 필요 시 정리 작업
        }
    }

    [System.Serializable]
    public class FullscreenFogSettings
    {
        public Material fogMaterial;
    }

    public FullscreenFogSettings settings = new FullscreenFogSettings();
    private FullscreenFogPass fogPass;

    public override void Create()
    {
        fogPass = new FullscreenFogPass(settings.fogMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(fogPass);
    }
}
