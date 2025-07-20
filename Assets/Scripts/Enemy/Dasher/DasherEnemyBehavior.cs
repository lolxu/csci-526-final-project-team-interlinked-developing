using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DasherEnemyBehavior : BaseEnemyBehavior
{
    [Header("Dasher Enemy Settings")]
    [SerializeField] private GameObject m_dashPickup;

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
            Instantiate(m_dashPickup, transform.position, Quaternion.identity);
        }
    }
}
