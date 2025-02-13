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
            // SingletonMaster.Instance.FeelManager.m_cameraShake.PlayFeedbacks();
            SingletonMaster.Instance.CameraShakeManager.Shake(2.5f, 0.1f);
        }
    }

    private void FixedUpdate()
    {
        m_RB.velocity = m_direction * m_speed * Time.fixedDeltaTime;
        m_lifeTime -= Time.fixedDeltaTime;
        if (m_lifeTime < 0.0f)
        {
            Destroy(gameObject);
        }
        
        if (m_bulletType == BulletType.Scanhit && !m_hasRaycasted)
        {
            m_hasRaycasted = true;
            RaycastHit2D[] hitTargets = Physics2D.RaycastAll(transform.position, m_direction, 100.0f, LayerMask.GetMask("Enemy"));

            int hitCount = m_penetrateNum;
            foreach (var hit in hitTargets)
            {
                if (hitCount > 0)
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        BaseEnemyBehavior enemy = hit.collider.gameObject.GetComponent<BaseEnemyBehavior>();
                        if (enemy)
                        {
                            enemy.EnemyDamagedEvent.Invoke(m_damage);
                            hit.rigidbody.AddForce(m_direction * m_knockback, ForceMode2D.Impulse);
                        }
                    }
                    hitCount--;
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
                if (m_bulletTargetTag == "Enemy")
                {
                    BaseEnemyBehavior enemy = other.gameObject.GetComponent<BaseEnemyBehavior>();
                    if (enemy)
                    {
                        enemy.EnemyDamagedEvent.Invoke(m_damage);
                        other.GetComponent<Rigidbody2D>().AddForce(m_direction * m_knockback, ForceMode2D.Impulse);
                    }
                }

                if (m_bulletTargetTag == "Player")
                {
                    HealthComponent health = other.gameObject.GetComponent<HealthComponent>();
                    if (health)
                    {
                        health.DamageEvent.Invoke(m_damage, m_owner);
                    }
                }
            }
            
            m_penetrateNum--;
            if (m_penetrateNum == 0 && other.CompareTag(m_bulletTargetTag))
            {
                m_trail.transform.SetParent(null, true);
                Destroy(gameObject);
            }
        }
    }
    
    
}
