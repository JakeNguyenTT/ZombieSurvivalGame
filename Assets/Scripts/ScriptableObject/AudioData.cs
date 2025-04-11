using UnityEngine;

[CreateAssetMenu(fileName = "NewAudio", menuName = "Audio/New Audio")]
public class AudioData : ScriptableObject
{
    public AudioID audioId; // Now an enum instead of string
    public AudioClip clip;
    public float volume = 1f;
    public float pitch = 1f;
    public bool isBGM = false;
}