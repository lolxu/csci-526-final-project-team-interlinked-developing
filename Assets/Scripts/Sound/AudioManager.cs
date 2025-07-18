using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public AudioSource sfxPlayerSource;
    public AudioSource sfxOtherSource;
    public AudioSource musicSource;
    public AudioSource uiSource;

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
        public AudioClip m_musicClip;
    }

    [Serializable]
    public class MusicLibrary
    {
        public List<MusicEntry> m_musicEntries = new List<MusicEntry>();
    }

    public AudioLibrary m_audioLibrary;
    public MusicLibrary m_musicLibrary;

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

    public void PlayUI(string audioName, AudioClip clip = null)
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
                musicSource.clip = entry.m_musicClip;
                musicSource.loop = loop;
                musicSource.Play();
            }
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}