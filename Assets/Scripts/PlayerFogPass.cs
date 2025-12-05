using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PlayerFogPass : ScriptableRenderPass
{
    public Material fogMaterial;
    public Transform player;
    public float fogRadius = 5f;
    public float grayscaleAmount = 0f;
    public float fogStartDistance = 8f;
    public float maxFogDensity = 0.85f;

    class PassData
    {
        public Material fogMaterial;
        public Vector3 playerPos;
        public float fogRadius;
        public float grayscaleAmount;
        public float fogStartDistance;
        public float maxFogDensity;
        public TextureHandle source;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (fogMaterial == null || player == null)
            return;

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        TextureHandle source = resourceData.activeColorTexture;

        var descriptor = cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        TextureHandle dest = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "FogTemp", false);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Player Fog Pass", out var passData))
        {
            passData.fogMaterial = fogMaterial;
            passData.playerPos = player.position;
            passData.fogRadius = fogRadius;
            passData.grayscaleAmount = grayscaleAmount;
            passData.fogStartDistance = fogStartDistance;
            passData.maxFogDensity = maxFogDensity;
            passData.source = source;

            builder.UseTexture(source);
            builder.SetRenderAttachment(dest, 0);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                data.fogMaterial.SetVector("_PlayerPos", data.playerPos);
                data.fogMaterial.SetFloat("_FogRadius", data.fogRadius);
                data.fogMaterial.SetFloat("_GrayscaleAmount", data.grayscaleAmount);
                data.fogMaterial.SetFloat("_FogStartDistance", data.fogStartDistance);
                data.fogMaterial.SetFloat("_MaxFogDensity", data.maxFogDensity);
                data.fogMaterial.SetTexture("_MainTex", data.source);
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.fogMaterial, 0);
            });
        }

        resourceData.cameraColor = dest;
    }
}