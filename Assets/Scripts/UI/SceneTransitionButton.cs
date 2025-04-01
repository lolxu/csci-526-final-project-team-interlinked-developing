using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionButton : MonoBehaviour
{
    public string m_loadScene;
    public bool m_showTutorial = true;
    public LevelDataScriptable m_levelData;
    
    public void TransitionScene()
    {
        m_levelData.m_needsTutorial = m_showTutorial;
        SceneManager.LoadScene(m_loadScene);
    }
}
