using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KillingSpreeManager : MonoBehaviour
{
    [Header("Killing Spree Settings")] 
    [SerializeField] private float m_killTimeout = 3.0f;
    
    private bool m_hasKillingSpree = false;
    private int m_killCount = 0;
    private float m_timer = 0.0f;
    
    void Start()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(AddToKillingSpree);
    }

    void OnDisable()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(AddToKillingSpree);
    }
    
    private void AddToKillingSpree(GameObject enemy)
    {
        m_killCount++;
        m_timer = 0.0f;

        if (!m_hasKillingSpree && m_killCount >= 5)
        {
            m_hasKillingSpree = true;
            SingletonMaster.Instance.EventManager.KillingSpreeStartEvent.Invoke();
            SingletonMaster.Instance.FeelManager.m_killingSpreeStart.PlayFeedbacks();
        }
    }

    private void Update()
    {
        m_timer += Time.deltaTime;
        if (m_timer >= m_killTimeout)
        {
            m_killCount = 0;

            if (m_hasKillingSpree)
            {
                SingletonMaster.Instance.EventManager.KillingSpreeEndEvent.Invoke();
                SingletonMaster.Instance.FeelManager.m_killingSpreeEnd.PlayFeedbacks();
                m_hasKillingSpree = false;
            }
            
        }
    }
}
