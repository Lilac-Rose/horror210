using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimatorUI : MonoBehaviour
{
    public Sprite[] frames;
    public float framesPerSecond = 10f;
    private Image image;
    private int currentFrame;
    private float timer;

    void Start()
    {
        image = GetComponent<Image>();
        if (frames.Length > 0)
            image.sprite = frames[0];
    }

    void Update()
    {
        if (frames.Length == 0) return;
        timer += Time.deltaTime;
        if (timer >= 1f / framesPerSecond)
        {
            timer -= 1f / framesPerSecond;
            currentFrame = (currentFrame + 1) % frames.Length;
            image.sprite = frames[currentFrame];
        }
    }
}