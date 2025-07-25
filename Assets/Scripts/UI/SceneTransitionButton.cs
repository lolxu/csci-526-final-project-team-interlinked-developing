using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionButton : MonoBehaviour
{
    public string m_loadScene;
    public bool m_showTutorial = false;
    public bool m_showLevelName = false;
    public LevelDataScriptable m_levelData;

    [SerializeField] private GameObject m_transitionSFX;

    private void OnEnable()
    {
        if (!m_showTutorial)
        {
            int ind = m_levelData.FindLevelIndex(m_loadScene);
            if (!m_levelData.m_levelUnlocked[ind])
            {
                GetComponent<Button>().interactable = false;
            }
            else
            {
                GetComponent<Button>().interactable = true;
            }
        }
    }

    public void TransitionScene()
    {
        Time.timeScale = 1.0f;

        if (m_loadScene == "Main Menu")
        {
            MetricsManager.Instance.Send();
        }

        Instantiate(m_transitionSFX);
        // AudioListener.pause = false;
        
        m_levelData.m_needsTutorial = m_showTutorial;
        m_levelData.m_needsLevelName = m_showLevelName;
        SceneManager.LoadScene(m_loadScene);
    }
}
