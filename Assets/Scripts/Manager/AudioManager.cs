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
    [SerializeField] private Queue<AudioSource> m_SFXPool;
    [SerializeField] private List<AudioSource> m_ActiveLoopingSFX;

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

    public void PlayBGM(AudioID audioId, bool fade = false)
    {
        AudioData config = m_AudioLibrary.GetItem(audioId);
        float targetVolume = config.volume * m_MusicVolume;
        if (config != null && config.isBGM)
        {
            if (fade)
            {
                FadeBGM(config.clip, targetVolume);
            }
            else
            {
                m_MusicSource.clip = config.clip;
                m_MusicSource.volume = targetVolume;
                m_MusicSource.Play();
            }
        }
    }

    private void FadeBGM(AudioClip newClip, float targetVolume)
    {
        float fadeDuration = 1f;

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

    public void PlaySFX(AudioID audioId, Vector3 position, float volume = 1f, float pitch = 1f, bool spatial = true)
    {
        var audioData = m_AudioLibrary.GetItem(audioId);
        float targetVolume = audioData.volume * m_SFXVolume * volume;
        if (audioData == null) return;

        if (m_SFXPool.Count == 0)
            AddSFXToPool();

        AudioSource source = m_SFXPool.Dequeue();
        SetupSource(source, audioData.clip, position, targetVolume, pitch, spatial, loop: false);
        source.Play();
        StartCoroutine(ReturnAfterPlay(source, audioData.clip.length / pitch));
    }

    private void SetupSource(AudioSource source, AudioClip clip, Vector3 position, float volume, float pitch, bool spatial, bool loop)
    {
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.spatialBlend = spatial ? 1f : 0f;
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
        source.clip = null;
        source.gameObject.SetActive(false);
        m_SFXPool.Enqueue(source);
    }
}