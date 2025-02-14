using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : MonoBehaviour
{
    public float m_damage = 2.5f;
    public float m_timeOffset = 10.0f;
    public bool m_canMove = false;

    private void OnCollisionStay2D(Collision2D other)
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

    private void Start()
    {
    }

    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
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
}
