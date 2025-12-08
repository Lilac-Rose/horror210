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

    [Header("Fade Settings")]
    [Tooltip("Enable fade in when playing sound")]
    public bool fadeIn = true;
    [Tooltip("Duration of fade in effect (seconds)")]
    public float fadeInDuration = 1f;

    private AudioSource audioSource;
    private float targetVolume;
    private bool isFading = false;

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
        audioSource.loop = loop;
        audioSource.playOnAwake = false;

        // Store target volume
        targetVolume = volume;
    }

    void Update()
    {
        // Update target volume if changed in Inspector (only when not playing)
        if (!audioSource.isPlaying)
        {
            targetVolume = volume;
        }

        // Handle fade in
        if (isFading && audioSource != null)
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, (targetVolume / fadeInDuration) * Time.deltaTime);

            // Stop fading when target reached
            if (Mathf.Approximately(audioSource.volume, targetVolume))
            {
                isFading = false;
            }
        }
    }

    // Public method to play the sound
    public void PlaySound()
    {
        if (audioSource != null && soundClip != null)
        {
            // Update target volume from inspector value
            targetVolume = volume;

            if (fadeIn)
            {
                // Start at zero volume and fade in
                audioSource.volume = 0f;
                isFading = true;
            }
            else
            {
                // Start at full volume
                audioSource.volume = targetVolume;
                isFading = false;
            }

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
            isFading = false;
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