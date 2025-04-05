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
    
    private GameObject SingletonMasterObject;

    private bool m_playerWon = false;
    private int m_curLevel = 1;
    private bool m_canRestart = false;
    
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_playerWon = false;
        SingletonMasterObject = GameObject.Find("Singleton Master");

        // TODO: This is bad...
        SingletonMaster.Instance.PlayerAbilities.ResetAbilities();
        SingletonMaster.Instance.EventManager.LevelClearEvent.AddListener(OnPlayerWin);
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LevelClearEvent.RemoveListener(OnPlayerWin);
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
    }

    private void OnPlayerWin()
    {
        m_playerWon = true;
    }
    
    private void OnPlayerDeath(GameObject arg0)
    {
        m_canRestart = true;
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
                    m_curLevel++;
                    SceneManager.LoadScene(m_levelData.m_levelNames[m_curLevel]);
                }
                else
                {
                    SingletonMaster.Instance.EventManager.PlayerWinEvent.Invoke();
                }
            }
        }
    }
}
