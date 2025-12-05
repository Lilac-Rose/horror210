using UnityEngine;

public class PlayerFogController : MonoBehaviour
{
    public Material fogMaterial;
    public Transform player;
    public float fogRadius = 5f; // How far the player can see

    void Update()
    {
        if (fogMaterial != null && player != null)
        {
            fogMaterial.SetVector("_PlayerPos", player.position);
            fogMaterial.SetFloat("_FogRadius", fogRadius);
        }
    }
}
