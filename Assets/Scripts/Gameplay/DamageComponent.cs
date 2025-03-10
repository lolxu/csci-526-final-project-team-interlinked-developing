using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : MonoBehaviour
{
    public float m_damage = 2.5f;
    public float m_timeOffset = 10.0f;
    public bool m_canMove = false;
   
    public bool m_oneTimeDamage = false;
    private bool m_canDamage = true;
    
    private void OnCollisionStay2D(Collision2D other)
    {
        if (m_canDamage)
        {
            if (other.collider.CompareTag("Enemy") || other.collider.CompareTag("Player"))
            {
                var health = other.gameObject.GetComponent<HealthComponent>();
                if (health != null)
                {
                    health.DamageEvent.Invoke(m_damage, gameObject);
                }
            }
        }

        if (m_oneTimeDamage)
        {
            m_canDamage = false;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_canDamage)
        {
            if (other.CompareTag("Player"))
            {
                var health = other.gameObject.GetComponent<HealthComponent>();
                if (health != null)
                {
                    health.DamageEvent.Invoke(m_damage, gameObject);
                }
            }
        }

        if (m_oneTimeDamage)
        {
            m_canDamage = false;
        }
    }
}
