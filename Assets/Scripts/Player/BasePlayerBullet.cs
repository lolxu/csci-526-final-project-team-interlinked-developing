using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayerBullet : MonoBehaviour
{
    public enum BulletType
    {
        Projectile,
        Scanhit
    }
    
    // Needs to be object pooled perhaps
    public string m_bulletTargetTag;
    public BulletType m_bulletType;
    public LayerMask m_scanHitLayerMask;
    public float m_damage = 4.0f;
    public float m_speed;
    public float m_lifeTime = 5.0f;
    public Vector2 m_direction;
    public int m_penetrateNum = 1;
    public float m_knockback = 10.0f;
    
    public int m_fireRate = 100;
    public float m_recoil = 10.0f;

    public GameObject m_trail;
    public GameObject m_owner;
    
    private Rigidbody2D m_RB;
    private bool m_hasRaycasted = false;

    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();

        if (m_bulletType == BulletType.Scanhit)
        {
            // SingletonMaster.Instance.FeelManager.m_cameraShake.PlayFeedbacks(Vector3.zero, 0.25f);
            SingletonMaster.Instance.CameraShakeManager.Shake(5.0f, 0.15f);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        m_RB.velocity = m_direction * m_speed * Time.deltaTime;
        // m_lifeTime -= Time.deltaTime;
        // if (m_lifeTime < 0.0f)
        // {
        //     Destroy(gameObject);
        // }
        
        if (m_bulletType == BulletType.Scanhit && !m_hasRaycasted)
        {
            m_hasRaycasted = true;
            RaycastHit2D[] hitTargets = Physics2D.RaycastAll(transform.position, m_direction, 100.0f, m_scanHitLayerMask);

            int hitCount = m_penetrateNum;
            foreach (var hit in hitTargets)
            {
                if (hitCount > 0)
                {
                    if (hit.collider.CompareTag(m_bulletTargetTag))
                    {
                        HealthComponent health = hit.collider.gameObject.GetComponent<HealthComponent>();
                        if (health)
                        {
                            health.DamageEvent.Invoke(m_damage, m_owner);
                        }
                        
                        hitCount--;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_bulletType == BulletType.Projectile && other.gameObject != m_owner)
        {
            Debug.Log("Hit " + other.gameObject);
            
            if (other.CompareTag(m_bulletTargetTag))
            {
                HealthComponent health = other.gameObject.GetComponent<HealthComponent>();
                if (health)
                {
                    health.DamageEvent.Invoke(m_damage, m_owner);
                }
            }
            
            m_penetrateNum--;
            if (m_penetrateNum == 0 && other.CompareTag(m_bulletTargetTag))
            {
                // m_trail.transform.SetParent(null, true);
                Destroy(gameObject);
            }
        }
    }
    
    
}
