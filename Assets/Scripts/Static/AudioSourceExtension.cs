using UnityEngine;


public static class AudioSourceExtension
{
    static public void Play(AudioClip clip, float volume = 1, float pitch = 1)
    {
        if (clip == null)
            return;

        AudioSource audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        audioSource.Play();

        GameObject.Destroy(audioSource.gameObject, clip.length / pitch);
    }
}
