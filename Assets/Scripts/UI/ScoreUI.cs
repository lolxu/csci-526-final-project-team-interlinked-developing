using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI m_text;

    private int m_killCount = 0;
    
    private void Start()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(AddKill);
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(AddKill);
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
    }

    private void Update()
    {
        
    }

    private void AddKill(GameObject enemy)
    {
        m_killCount++;
        m_text.text = m_killCount.ToString();
    }

    private void OnPlayerDeath(GameObject killer)
    {
        BaseEnemyBehavior enemy = killer.GetComponent<BaseEnemyBehavior>();
        m_text.text = "Killed by " + enemy.m_name;
    }
}
