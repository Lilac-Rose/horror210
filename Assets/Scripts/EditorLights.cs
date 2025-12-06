using UnityEngine;

public class EditorOnlyLights : MonoBehaviour
{
    void Awake()
    {
        // Only keep lights active when NOT playing
        if (Application.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }
}