using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class GodRaysRenderFeature : ScriptableRendererFeature
{
    private GodRaysRenderPass godRaysRenderPass;

    [SerializeField] private Shader occlusionShader;
    [SerializeField] private Shader blurShader;
    [SerializeField] private Shader compositeShader;

    [Header("Light Settings")]
    [SerializeField] private float defaultLightRadius = 0.05f;

    [Header("Blur Settings")]
    [SerializeField, Range(1f, 3f)] private float totalDistance = 1.35f;
    [SerializeField, Range(1, 128)] private int samples = 45;
    [SerializeField, Range(0f, 1f)] private float decay = 0.9f;
    [SerializeField] private float blurIntensity = 0.1f;

    [Header("Composite Settings")]
    [SerializeField] private float rayIntensity = 0.85f;

    Material blurMaterial;
    Material occlusionMaterial;
    Material compositeMaterial;

    private static readonly int LightScreenPosesID = Shader.PropertyToID("_LightScreenPoses");
    private static readonly int LightRadiusesID = Shader.PropertyToID("_LightRadiuses");
    private static readonly int LightCountID = Shader.PropertyToID("_LightCount");
    private static readonly int SunRadiusID = Shader.PropertyToID("_SunRadius");
    private static readonly int RayIntensityID = Shader.PropertyToID("_RayIntensity");
    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int TotalDistanceID = Shader.PropertyToID("_TotalDistance");
    private static readonly int SamplesID = Shader.PropertyToID("_Samples");
    private static readonly int DecayID = Shader.PropertyToID("_Decay");

    private const int MAX_GODRAY_LIGHTS = 8;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (blurMaterial == null || occlusionMaterial == null || compositeMaterial == null|| LightManager.Instance == null)
            return;
        
        List<Transform> lights = LightManager.Instance.godRayLightsTransforms;
        List<float> radiusesList = LightManager.Instance.godRayLightsRadiuses;
        if (lights == null || lights.Count == 0)
            return;

        Camera camera = renderingData.cameraData.camera;
        int count = Mathf.Min(lights.Count, MAX_GODRAY_LIGHTS);
        // Convert to screen positions to make god rays less apparent the further away they are
        Vector4[] screenPositions = new Vector4[MAX_GODRAY_LIGHTS];
        float[] radiuses = new float[MAX_GODRAY_LIGHTS];
        int writeIndex = 0;
        for (int i = 0; i < count; i++)
        {
            if (lights[i] == null)
            {
                count--;
                continue;
            }
            Vector3 viewportPos = camera.WorldToViewportPoint(lights[i].position);
            screenPositions[writeIndex] = new Vector4(viewportPos.x, viewportPos.y, 0, 0);
            radiuses[writeIndex] = i >= radiusesList.Count ? defaultLightRadius : radiusesList[i];
            writeIndex++;
        }

        // Occlusion pass parameters
        occlusionMaterial.SetInt(LightCountID, count);
        occlusionMaterial.SetVectorArray(LightScreenPosesID, screenPositions);
        occlusionMaterial.SetFloatArray(LightRadiusesID, radiuses);

        // Composite pass parameters
        compositeMaterial.SetInt(LightCountID, count);
        compositeMaterial.SetVectorArray(LightScreenPosesID, screenPositions);
        compositeMaterial.SetFloatArray(LightRadiusesID, radiuses);
        compositeMaterial.SetFloat(RayIntensityID, rayIntensity);

        // Blur pass parameters
        blurMaterial.SetInt(LightCountID, count);
        blurMaterial.SetVectorArray(LightScreenPosesID, screenPositions);
        blurMaterial.SetFloat(IntensityID, blurIntensity);
        blurMaterial.SetFloat(TotalDistanceID, totalDistance);
        blurMaterial.SetInt(SamplesID, samples);
        blurMaterial.SetFloat(DecayID, decay);

        godRaysRenderPass.ConfigureInput(ScriptableRenderPassInput.Depth);
        renderer.EnqueuePass(godRaysRenderPass);
    }

    public override void Create()
    {
        // Apparently I need to destory these materials during create to prevent the blitter error. Idk why
        CoreUtils.Destroy(blurMaterial);
        CoreUtils.Destroy(occlusionMaterial);
        CoreUtils.Destroy(compositeMaterial);

        blurMaterial = CoreUtils.CreateEngineMaterial(blurShader);
        occlusionMaterial = CoreUtils.CreateEngineMaterial(occlusionShader);
        compositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);

        godRaysRenderPass = new GodRaysRenderPass(blurMaterial, compositeMaterial, occlusionMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
        godRaysRenderPass.ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(blurMaterial);
        CoreUtils.Destroy(occlusionMaterial);
        CoreUtils.Destroy(compositeMaterial);
    }

    public class GodRaysRenderPass : ScriptableRenderPass
    {
        private Material blurMaterial;
        private Material compositeMaterial;
        private Material occlusionMaterial;

        public GodRaysRenderPass(Material blurMaterial, Material compositeMaterial, Material occlusionMaterial)
        {
            this.blurMaterial = blurMaterial;
            this.compositeMaterial = compositeMaterial;
            this.occlusionMaterial = occlusionMaterial;
        }

        private class BlurPassData
        {
            public TextureHandle source;
            public Material material;
        }

        private class CompositePassData
        {
            public TextureHandle godRays;
            public TextureHandle sceneColor;
            public Material material;
        }

        private class OcclusionPassData
        {
            public TextureHandle source;
            public Material material;
        }

        static void ExecuteOcclusionPass(OcclusionPassData data, RasterGraphContext context)
        {
            Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
        }

        static void ExecuteBlurPass(BlurPassData data, RasterGraphContext context)
        {
            Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
        }

        static void ExecuteCompositePass(CompositePassData data, RasterGraphContext context)
        {
            data.material.SetTexture("_GodRaysTex", data.godRays);
            Blitter.BlitTexture(context.cmd, data.sceneColor, new Vector4(1, 1, 0, 0), data.material, 0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            TextureHandle cameraColor = resourceData.activeColorTexture;
            TextureHandle cameraDepth = resourceData.cameraDepthTexture;

            var occDesc = renderGraph.GetTextureDesc(cameraColor);
            occDesc.name = "_OcclusionMask";
            occDesc.clearBuffer = false;
            TextureHandle occlusionMask = renderGraph.CreateTexture(occDesc);

            using (var builder = renderGraph.AddRasterRenderPass<OcclusionPassData>("God Rays Occlusion", out var passData))
            {
                passData.source = cameraDepth;
                passData.material = occlusionMaterial;
                builder.UseTexture(cameraDepth);
                builder.SetRenderAttachment(occlusionMask, 0);
                builder.SetRenderFunc((OcclusionPassData data, RasterGraphContext ctx) => ExecuteOcclusionPass(data, ctx));
            }

            var blurDesc = renderGraph.GetTextureDesc(cameraColor);
            blurDesc.name = "_GodRaysBlurred";
            blurDesc.clearBuffer = false;
            TextureHandle godRaysResult = renderGraph.CreateTexture(blurDesc);

            using (var builder = renderGraph.AddRasterRenderPass<BlurPassData>("God Rays Blur", out var passData))
            {
                builder.AllowGlobalStateModification(true);
                passData.source = occlusionMask;
                passData.material = blurMaterial;
                builder.UseTexture(occlusionMask);
                builder.SetRenderAttachment(godRaysResult, 0);
                builder.SetRenderFunc((BlurPassData data, RasterGraphContext ctx) => ExecuteBlurPass(data, ctx));
            }

            var compositeDesc = renderGraph.GetTextureDesc(cameraColor);
            compositeDesc.name = "_GodRaysComposite";
            compositeDesc.clearBuffer = false;
            TextureHandle composite = renderGraph.CreateTexture(compositeDesc);

            using (var builder = renderGraph.AddRasterRenderPass<CompositePassData>("God Rays Composite", out var passData))
            {
                builder.AllowGlobalStateModification(true);
                passData.godRays = godRaysResult;
                passData.sceneColor = cameraColor;
                passData.material = compositeMaterial;
                builder.UseTexture(godRaysResult);
                builder.UseTexture(cameraColor);
                builder.SetRenderAttachment(composite, 0);
                builder.SetRenderFunc((CompositePassData data, RasterGraphContext ctx) => ExecuteCompositePass(data, ctx));
            }

            renderGraph.AddCopyPass(composite, cameraColor, passName: "Copy God Rays To Screen");
        }
    }
}