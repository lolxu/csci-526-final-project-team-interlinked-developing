using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public LevelDataScriptable m_levelData;
    
    public bool m_gamePaused = false;
    
    private GameObject SingletonMasterObject;

    private bool m_playerWon = false;
    private bool m_canRestart = false;
    private bool m_canPause = true;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        } 
        else 
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_playerWon = false;
        SingletonMasterObject = GameObject.Find("Singleton Master");

        if (scene.name == "Main Menu")
        {
            m_canPause = false;
        }
        else
        {
            m_canPause = true;
            
            m_levelData.SetLevelUnlocked(scene.name);
        }

        // TODO: This is bad...
        if (SingletonMasterObject != null)
        {
            SingletonMaster.Instance.PlayerAbilities.ResetAbilities();
            SingletonMaster.Instance.EventManager.LevelClearEvent.AddListener(OnPlayerLevelClear);
            SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);

            SingletonMaster.Instance.PlayerBase.m_ropeRangeIndicator.SetActive(m_levelData.m_needsRopeRangeIndicator);
            
            PlayMusicBasedOnScene(scene.name);
        }
        else
        {
            Debug.LogError("No Singleton Instances found...");
        }
    }

    private void PlayMusicBasedOnScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Cavern":
                SingletonMaster.Instance.AudioManager.PlayMusic("MusicCavern");
                break;
            case "Vines":
                SingletonMaster.Instance.AudioManager.PlayMusic("MusicVines");
                break;
            case "Factory":
                SingletonMaster.Instance.AudioManager.PlayMusic("MusicFactory");
                break;
        }
        
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LevelClearEvent.RemoveListener(OnPlayerLevelClear);
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
    }

    private void OnPlayerLevelClear()
    {
        m_playerWon = true;
        m_levelData.SetLevelUnlocked(SceneManager.GetActiveScene().name);
        
        // Stop Music
        SingletonMaster.Instance.AudioManager.StopMusic();
    }
    
    private void OnPlayerDeath(GameObject arg0)
    {
        m_canRestart = true;
        SingletonMaster.Instance.AudioManager.StopMusic();
    }

    private void Update()
    {
        // FOR DEBUG ONLY
        if (Input.GetKeyDown(KeyCode.R) && m_canRestart)
        {
            SingletonMaster.Instance.PlayerAbilities.ResetAbilities();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            m_canRestart = false;
        }

        if (m_playerWon)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (SceneManager.GetActiveScene().buildIndex < m_levelData.m_levelNames.Count - 1)
                {
                    // Show level name for new level
                    m_levelData.m_needsLevelName = true;
                    
                    SceneManager.LoadScene(m_levelData.m_levelNames[SceneManager.GetActiveScene().buildIndex + 1]);
                }
                else
                {
                    SingletonMaster.Instance.EventManager.PlayerWinEvent.Invoke();
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && !m_gamePaused && m_canPause)
        {
            m_gamePaused = true;
            SingletonMaster.Instance.UI.PauseGame();
        }
    }
}
