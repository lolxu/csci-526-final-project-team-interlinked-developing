using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitchTrigger : MonoBehaviour
{
    [SerializeField] private string m_loadSceneName = "";
    [SerializeField] private float m_stayTimer = 5.0f;
    private bool m_isStaying = false;
    private float m_timer = 0.0f;
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            m_isStaying = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            m_isStaying = false;
        }
    }

    private void Update()
    {
        if (m_isStaying)
        {
            m_timer += Time.deltaTime;

            if (m_timer >= m_stayTimer)
            {
                m_timer = m_stayTimer;
                SceneManager.LoadScene(m_loadSceneName);
            }
        }
        else
        {
            m_timer = 0.0f;
        }
    }
}
