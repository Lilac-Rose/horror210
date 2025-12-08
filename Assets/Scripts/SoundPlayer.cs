using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("The audio clip to play")]
    public AudioClip soundClip;

    [Range(0f, 1f)]
    [Tooltip("Volume of the sound (0 to 1)")]
    public float volume = 1f;

    [Header("Playback Options")]
    [Tooltip("Loop the sound continuously")]
    public bool loop = false;

    private AudioSource audioSource;

    void Awake()
    {
        // Add an AudioSource component if one doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure the AudioSource
        audioSource.clip = soundClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        // Update volume in real-time if changed in Inspector
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    // Public method to play the sound
    public void PlaySound()
    {
        if (audioSource != null && soundClip != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Cannot play sound: AudioSource or AudioClip is missing!");
        }
    }

    // Public method to stop the sound
    public void StopSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    // Public method to pause the sound
    public void PauseSound()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    // Public method to resume the sound
    public void ResumeSound()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }
}