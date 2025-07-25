using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public AudioSource sfxPlayerSource;
    public AudioSource sfxOtherSource;
    public AudioSource musicSource;
    public AudioSource uiSource;

    private bool m_stopMusic = false;
    private bool m_playIntenseMusic = false;
    private Coroutine m_musicCoroutine;

    [Serializable]
    public class AudioEntry
    {
        public string m_audioName;
        public AudioClip m_audioClip;
    }
    
    [Serializable]
    public class AudioLibrary
    {
        public List<AudioEntry> m_audioEntries = new List<AudioEntry>();
    }

    [Serializable]
    public class MusicEntry
    {
        public string m_musicName;
        public List<AudioClip> m_musicClips = new List<AudioClip>();
        public List<AudioClip> m_intenseMusicClips = new List<AudioClip>();
        public AudioClip m_oneShotStartTransition;
        public AudioClip m_oneShotEndTransition;
    }

    [Serializable]
    public class MusicLibrary
    {
        public List<MusicEntry> m_musicEntries = new List<MusicEntry>();
    }

    public AudioLibrary m_audioLibrary;
    public MusicLibrary m_musicLibrary;

    private void Start()
    {
        SingletonMaster.Instance.EventManager.KillingSpreeStartEvent.AddListener(KillingSpreeStarted);
        SingletonMaster.Instance.EventManager.KillingSpreeEndEvent.AddListener(KillingSpreeEnded);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.KillingSpreeStartEvent.RemoveListener(KillingSpreeStarted);
        SingletonMaster.Instance.EventManager.KillingSpreeEndEvent.RemoveListener(KillingSpreeEnded);
    }

    private void KillingSpreeStarted()
    {
        m_playIntenseMusic = true;
    }

    private void KillingSpreeEnded()
    {
        m_playIntenseMusic = false;
    }

    public void PlayPlayerSFX(string audioName, AudioClip clip = null)
    {
        // Find the audio name first
        bool found = false;
        foreach (var entry in m_audioLibrary.m_audioEntries)
        {
            if (entry.m_audioName == audioName)
            {
                found = true;
                sfxPlayerSource.pitch = Random.Range(0.85f, 1.15f);
                sfxPlayerSource.volume = Random.Range(0.9f, 1.0f);
                sfxPlayerSource.PlayOneShot(entry.m_audioClip);
            }
        }
        
        if (!found && clip != null)
        {
            sfxPlayerSource.pitch = Random.Range(0.85f, 1.15f);
            sfxPlayerSource.volume = Random.Range(0.9f, 1.0f);
            sfxPlayerSource.PlayOneShot(clip);
        }
    }

    public void PlayOtherSFX(string audioName, AudioClip clip = null)
    {
        // Find the audio name first
        bool found = false;
        foreach (var entry in m_audioLibrary.m_audioEntries)
        {
            if (entry.m_audioName == audioName)
            {
                found = true;
                sfxOtherSource.pitch = Random.Range(0.85f, 1.15f);
                sfxOtherSource.volume = Random.Range(0.9f, 1.0f);
                sfxOtherSource.PlayOneShot(entry.m_audioClip);
            }
        }
        
        if (!found && clip != null)
        {
            sfxOtherSource.pitch = Random.Range(0.85f, 1.15f);
            sfxOtherSource.volume = Random.Range(0.9f, 1.0f);
            sfxOtherSource.PlayOneShot(clip);
        }
    }

    public void PlayUISFX(string audioName, AudioClip clip = null)
    {
        // Find the audio name first
        bool found = false;
        foreach (var entry in m_audioLibrary.m_audioEntries)
        {
            if (entry.m_audioName == audioName)
            {
                found = true;
                uiSource.pitch = Random.Range(0.85f, 1.15f);
                uiSource.volume = Random.Range(0.9f, 1.0f);
                uiSource.PlayOneShot(entry.m_audioClip);
            }
        }
        
        if (!found && clip != null)
        {
            uiSource.pitch = Random.Range(0.85f, 1.15f);
            uiSource.volume = Random.Range(0.9f, 1.0f);
            uiSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(string musicName, bool loop = true)
    {
        foreach (var entry in m_musicLibrary.m_musicEntries)
        {
            if (entry.m_musicName == musicName)
            {
                StartPlayingMusicClips(entry.m_musicClips, entry.m_intenseMusicClips, entry.m_oneShotStartTransition, entry.m_oneShotEndTransition);
            }
        }
    }

    private void StartPlayingMusicClips(List<AudioClip> m_musicClips, List<AudioClip> intenseMusicClips,  AudioClip oneShotStartTransition, AudioClip oneShotEndTransition)
    {
        m_stopMusic = false;
        m_musicCoroutine = StartCoroutine(MusicCoroutine(m_musicClips, intenseMusicClips, oneShotStartTransition, oneShotEndTransition));
    }

    private IEnumerator MusicCoroutine(List<AudioClip> m_musicClips, List<AudioClip> intenseMusicClips,  AudioClip oneShotStartTransition, AudioClip oneShotEndTransition)
    {
        bool isPlayingIntense = false;
        while (!m_stopMusic)
        {
            while (musicSource.isPlaying)
            {
                yield return null;
            }

            if (m_playIntenseMusic)
            {
                if (!isPlayingIntense)
                {
                    isPlayingIntense = true;
                    musicSource.PlayOneShot(oneShotStartTransition);
                }
                musicSource.clip = intenseMusicClips[Random.Range(0, intenseMusicClips.Count)];
                musicSource.Play();
            }
            else
            {
                if (isPlayingIntense)
                {
                    isPlayingIntense = false;
                    musicSource.PlayOneShot(oneShotEndTransition);
                }
                musicSource.clip = m_musicClips[Random.Range(0, m_musicClips.Count)];
                musicSource.Play();
            }
        }
    }

    public void StopMusic()
    {
        m_stopMusic = true;
        StopCoroutine(m_musicCoroutine);
        musicSource.Stop();
    }
}