using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class HammerComponent : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float m_damage = 10.0f;
    [SerializeField] private float m_velocityThreshold = 2.0f;
    [SerializeField] private float m_knockBackStrength = 100.0f;
    
    [Header("Visual Settings")] 
    [SerializeField] private TrailRenderer m_trail;

    private Rigidbody2D m_RB;
    private bool m_canDamage = false;
    
    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (m_RB.velocity.magnitude > m_velocityThreshold)
        {
            m_trail.enabled = true;
            m_canDamage = true;
        }
        else
        {
            m_trail.enabled = false;
            m_canDamage = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy") && m_canDamage)
        {
            BaseEnemyBehavior enemy = other.gameObject.GetComponent<BaseEnemyBehavior>();
            if (enemy != null)
            {
                Rigidbody2D enemyRB = enemy.gameObject.GetComponent<Rigidbody2D>();
                if (enemyRB != null)
                {
                    enemyRB.AddForce(m_RB.velocity.normalized * m_knockBackStrength, ForceMode2D.Impulse);
                }
                enemy.EnemyDamagedEvent.Invoke(m_damage);
            }
        }
    }
}
