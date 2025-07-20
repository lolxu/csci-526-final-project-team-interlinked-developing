using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HealerEnemyBehavior : BaseEnemyBehavior
{
    [Header("Healer Enemy Settings")]
    [SerializeField] private GameObject m_healthPickup;

    protected override void Start()
    {
        base.Start();
        
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(OnHealerDead);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(OnHealerDead);
    }

    private void OnHealerDead(GameObject enemy)
    {
        if (enemy == gameObject)
        {
            Instantiate(m_healthPickup, transform.position, Quaternion.identity);
        }
    }
}
