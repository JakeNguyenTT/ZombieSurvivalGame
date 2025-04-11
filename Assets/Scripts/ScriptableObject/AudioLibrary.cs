using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    public List<AudioData> m_AllAudio;

    private Dictionary<AudioID, AudioData> m_Map;

    public void Init()
    {
        m_Map = new Dictionary<AudioID, AudioData>();
        foreach (var data in m_AllAudio)
        {
            if (!m_Map.ContainsKey(data.audioId))
                m_Map.Add(data.audioId, data);
        }
    }

    public AudioData GetItem(AudioID audioId)
    {
        if (m_Map == null) Init();
        return m_Map.ContainsKey(audioId) ? m_Map[audioId] : null;
    }
}