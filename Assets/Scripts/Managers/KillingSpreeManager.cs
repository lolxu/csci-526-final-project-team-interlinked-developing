using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                m_hasKillingSpree = false;
            }
            
        }
    }
}
