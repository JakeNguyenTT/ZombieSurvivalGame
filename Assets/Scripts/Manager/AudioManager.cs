using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource m_MusicSource;
    [SerializeField] private AudioLibrary m_AudioLibrary;
    [SerializeField] private float m_MusicFadeDuration = 1f; // duration for fading

    [Range(0f, 1f)]
    [SerializeField] private float m_MusicVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float m_SFXVolume = 1f;

    [Header("SFX")]
    [SerializeField] private AudioSource m_SFXPrefab;
    [SerializeField] private int m_SFXPoolInitSize = 10;
    [SerializeField] private int m_MaxSameClipInstances = 4; // skip a clip once this many copies are playing
    private readonly Dictionary<AudioClip, int> m_ActiveClipCounts = new Dictionary<AudioClip, int>();
    [SerializeField] private Queue<AudioSource> m_SFXPool;
    [SerializeField] private List<AudioSource> m_ActiveLoopingSFX;
    private AudioData m_CurrentBGM;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        m_MusicSource.loop = true;
        m_AudioLibrary.Init();
        InitSFXPool();

        // Saved settings override the inspector defaults
        m_MusicVolume = SaveData.GetMusicVolume(m_MusicVolume);
        m_SFXVolume = SaveData.GetSfxVolume(m_SFXVolume);
    }

    public float MusicVolume => m_MusicVolume;
    public float SfxVolume => m_SFXVolume;

    public void SetMusicVolume(float volume)
    {
        m_MusicVolume = Mathf.Clamp01(volume);
        SaveData.SetMusicVolume(m_MusicVolume);
        if (m_CurrentBGM != null)
        {
            m_MusicSource.DOKill();
            m_MusicSource.volume = m_CurrentBGM.volume * m_MusicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        m_SFXVolume = Mathf.Clamp01(volume);
        SaveData.SetSfxVolume(m_SFXVolume);
    }

    private void InitSFXPool()
    {
        m_SFXPool = new Queue<AudioSource>();
        for (int i = 0; i < m_SFXPoolInitSize; i++)
        {
            AddSFXToPool();
        }
    }

    private void AddSFXToPool()
    {
        var sfxObject = Instantiate(m_SFXPrefab, transform);
        sfxObject.gameObject.SetActive(false);
        sfxObject.playOnAwake = false;
        m_SFXPool.Enqueue(sfxObject);
    }

    public void PlayBGM(AudioID audioId, bool fade = false, float fadeDuration = 1f)
    {
        AudioData config = m_AudioLibrary.GetItem(audioId);
        if (config != null && config.isBGM)
        {
            m_CurrentBGM = config;
            float targetVolume = config.volume * m_MusicVolume;
            if (fade)
            {
                FadeBGM(config.clip, targetVolume, fadeDuration);
            }
            else
            {
                m_MusicSource.clip = config.clip;
                m_MusicSource.volume = targetVolume;
                m_MusicSource.Play();
            }
        }
    }

    private void FadeBGM(AudioClip newClip, float targetVolume, float fadeDuration)
    {

        // If music is playing, fade out
        if (m_MusicSource.isPlaying)
        {
            m_MusicSource.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                m_MusicSource.Stop();
                m_MusicSource.clip = newClip;
                m_MusicSource.volume = 0f;
                m_MusicSource.Play();

                // Fade in to target volume
                m_MusicSource.DOFade(targetVolume, fadeDuration);
            });
        }
        else
        {
            // Directly set and fade in if not playing
            m_MusicSource.clip = newClip;
            m_MusicSource.volume = 0f;
            m_MusicSource.Play();
            m_MusicSource.DOFade(targetVolume, fadeDuration);
        }
    }

    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, bool spatial = true)
    {
        if (clip == null)
        {
            Debug.LogError("SFX is missing");
            return;
        }
        m_ActiveClipCounts.TryGetValue(clip, out int playing);
        if (playing >= m_MaxSameClipInstances) return;
        m_ActiveClipCounts[clip] = playing + 1;

        float targetVolume = volume * m_SFXVolume;
        if (m_SFXPool.Count == 0)
            AddSFXToPool();

        AudioSource source = m_SFXPool.Dequeue();
        SetupSource(source, clip, position, targetVolume, pitch, spatial, loop: false);
        source.Play();
        StartCoroutine(ReturnAfterPlay(source, clip.length / pitch));
    }

    public void PlaySFX(AudioID audioId, Vector3 position, float volume = 1f, float pitch = 1f, bool spatial = true)
    {
        var audioData = m_AudioLibrary.GetItem(audioId);
        if (audioData == null) return;
        // m_SFXVolume is applied inside PlaySFX(AudioClip, ...)
        PlaySFX(audioData.clip, position, audioData.volume * volume, pitch, spatial);
    }

    private void SetupSource(AudioSource source, AudioClip clip, Vector3 position, float volume, float pitch, bool spatial, bool loop)
    {
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        // source.spatialBlend = spatial ? 1f : 0f;
        source.gameObject.SetActive(true);
    }

    private IEnumerator ReturnAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!source.loop)
        {
            CleanupSource(source);
        }
    }

    private void CleanupSource(AudioSource source)
    {
        source.Stop();
        source.loop = false;
        if (source.clip != null && m_ActiveClipCounts.TryGetValue(source.clip, out int playing))
            m_ActiveClipCounts[source.clip] = Mathf.Max(0, playing - 1);
        source.clip = null;
        source.gameObject.SetActive(false);
        m_SFXPool.Enqueue(source);
    }
}