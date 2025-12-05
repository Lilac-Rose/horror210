using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerFogFeature : ScriptableRendererFeature
{
    public Material fogMaterial;
    public string playerTag = "Player";
    public float fogRadius = 5f;
    public float fogStartDistance = 8f;

    [Range(0f, 1f)]
    public float grayscaleAmount = 0f;

    [Range(0f, 1f)]
    public float maxFogDensity = 0.85f;

    PlayerFogPass fogPass;
    Transform player;

    public override void Create()
    {
        fogPass = new PlayerFogPass();
        fogPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        fogPass.fogMaterial = fogMaterial;
        fogPass.player = player;
        fogPass.fogRadius = fogRadius;
        fogPass.grayscaleAmount = grayscaleAmount;
        fogPass.fogStartDistance = fogStartDistance;
        fogPass.maxFogDensity = maxFogDensity;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
                player = playerObj.transform;
        }

        fogPass.fogMaterial = fogMaterial;
        fogPass.player = player;
        fogPass.fogRadius = fogRadius;
        fogPass.grayscaleAmount = grayscaleAmount;
        fogPass.fogStartDistance = fogStartDistance;
        fogPass.maxFogDensity = maxFogDensity;

        renderer.EnqueuePass(fogPass);
    }
}