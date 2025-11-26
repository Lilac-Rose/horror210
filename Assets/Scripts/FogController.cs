using UnityEngine;

public class FogController : MonoBehaviour
{
    public Material fogMaterial;   // The material using the shader
    public Light playerLight;      // Player's point light

    void Update()
    {
        fogMaterial.SetVector("_PlayerLightPosition", playerLight.transform.position);
    }
}
