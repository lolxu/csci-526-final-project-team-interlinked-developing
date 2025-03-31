using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionButton : MonoBehaviour
{
    public string m_loadScene;
    
    public void TransitionScene()
    {
        SceneManager.LoadScene(m_loadScene);
    }
}
