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
        SingletonMaster.Instance.PlayerAbilities.ResetAbilities();
        // SingletonMaster.Instance.EventManager.LevelClearEvent.RemoveListener(OnPlayerWin);
        // TODO: This is bad...
        SingletonMaster.Instance.EventManager.LevelClearEvent.AddListener(OnPlayerWin);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LevelClearEvent.RemoveListener(OnPlayerWin);
    }

    private void OnPlayerWin()
    {
        m_playerWon = true;
    }

    private void Update()
    {
        // FOR DEBUG ONLY
        if (Input.GetKeyDown(KeyCode.R))
        {
            SingletonMaster.Instance.PlayerAbilities.ResetAbilities();
            SingletonMasterObject.transform.SetParent(GameObject.FindWithTag("Garbage").transform, true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (m_playerWon)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_curLevel++;
                if (m_curLevel < m_levelData.m_levelNames.Count)
                {
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
