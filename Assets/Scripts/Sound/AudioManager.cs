using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxPlayerSource;
    public AudioSource sfxOtherSource;
    public AudioSource musicSource;
    public AudioSource uiSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayPlayerSFX(AudioClip clip)
    {
        if (clip != null) sfxPlayerSource.PlayOneShot(clip);
    }

    public void PlayOtherSFX(AudioClip clip)
    {
        if (clip != null) sfxOtherSource.PlayOneShot(clip);
    }

    public void PlayUI(AudioClip clip)
    {
        if (clip != null) uiSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}