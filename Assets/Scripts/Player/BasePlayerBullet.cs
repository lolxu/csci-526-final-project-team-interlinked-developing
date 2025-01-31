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
    
    private Rigidbody2D m_RB;
    private bool m_hasRaycasted = false;

    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();

        if (m_bulletType == BulletType.Scanhit)
        {
            // SingletonMaster.Instance.FeelManager.m_cameraShake.PlayFeedbacks();
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
        if (m_bulletType == BulletType.Projectile)
        {
            Debug.Log("Hit " + other.gameObject);
            
            if (other.CompareTag("Enemy"))
            {
                BaseEnemyBehavior enemy = other.gameObject.GetComponent<BaseEnemyBehavior>();
                if (enemy)
                {
                    enemy.EnemyDamagedEvent.Invoke(m_damage);
                    other.GetComponent<Rigidbody2D>().AddForce(m_direction * m_knockback, ForceMode2D.Impulse);
                }
            }
            
            m_penetrateNum--;
            if (m_penetrateNum == 0)
            {
                m_trail.transform.SetParent(null, true);
                Destroy(gameObject);
            }
        }
    }
    
    
}
