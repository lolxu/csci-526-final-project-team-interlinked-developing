using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public float m_damage = 2.5f;

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
}
