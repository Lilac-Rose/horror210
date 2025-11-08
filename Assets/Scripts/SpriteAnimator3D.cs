using UnityEngine;

public class SpriteAnimator3D : MonoBehaviour
{
    public Texture2D[] frames;      // Drag your frames here in order
    public float framesPerSecond = 10f;

    private Renderer rend;
    private int currentFrame;
    private float timer;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (frames.Length > 0)
            rend.material.mainTexture = frames[0];
    }

    void Update()
    {
        if (frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / framesPerSecond)
        {
            timer -= 1f / framesPerSecond;
            currentFrame = (currentFrame + 1) % frames.Length;
            rend.material.mainTexture = frames[currentFrame];
        }
    }
}
