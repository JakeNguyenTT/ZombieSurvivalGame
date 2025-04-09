using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private List<AudioClip> clips;

    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var clip in clips)
            audioClips[clip.name] = clip;
    }

    public void PlayMusic(string clipName)
    {
        if (audioClips.TryGetValue(clipName, out AudioClip clip))
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(string clipName)
    {
        if (audioClips.TryGetValue(clipName, out AudioClip clip))
            sfxSource.PlayOneShot(clip);
    }
}