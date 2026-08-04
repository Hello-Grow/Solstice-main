using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class PreOpaqueFullScreenFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public int materialPassIndex = 0;
    }

    public Settings settings = new Settings();
    private PreOpaqueRenderPass m_RenderPass;

    public override void Create()
    {
        m_RenderPass = new PreOpaqueRenderPass(settings.material, settings.materialPassIndex);
        m_RenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;
        renderer.EnqueuePass(m_RenderPass);
    }

    class PreOpaqueRenderPass : ScriptableRenderPass
    {
        private Material m_Material;
        private int m_PassIndex;

        public PreOpaqueRenderPass(Material mat, int pass)
        {
            m_Material = mat;
            m_PassIndex = pass;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (cameraData.camera.cameraType == CameraType.Preview) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            TextureHandle colorTarget = resourceData.activeColorTexture;

            RenderGraphUtils.BlitMaterialParameters blitParams = new RenderGraphUtils.BlitMaterialParameters(
                colorTarget,
                colorTarget,
                m_Material,
                m_PassIndex
            );

            renderGraph.AddBlitPass(blitParams);
        }
    }
}
