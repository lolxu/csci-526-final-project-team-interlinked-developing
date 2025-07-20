using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RepulserEnemyBehavior : BaseEnemyBehavior
{
    [Header("Repulser Enemy Settings")]
    [SerializeField] private GameObject m_repulsePickup;

    protected override void Start()
    {
        base.Start();
        
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(OnEnemyDead);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(OnEnemyDead);
    }

    private void OnEnemyDead(GameObject enemy)
    {
        if (enemy == gameObject)
        {
            Instantiate(m_repulsePickup, transform.position, Quaternion.identity);
        }
    }
}
