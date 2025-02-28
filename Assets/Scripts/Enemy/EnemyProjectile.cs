using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")] 
    public float m_acceleration = 30.0f;
    public float m_maxSpeed = 50.0f;
    public float m_damage = 4.0f;
    public Vector2 m_moveDirection;
    [SerializeField] private LayerMask m_affectedLayer;
    
    private Rigidbody2D m_RB;
    private Collider2D m_collider;
    private RopeComponent m_rope;
    private bool m_canMove = true;
    private bool m_isDoneSpawning = false;
    
    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
        m_rope = GetComponent<RopeComponent>();
        m_collider = GetComponent<Collider2D>();
        
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
    }

    private void OnDisable()
    {
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            m_rope.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
        }
        
        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
    }

    public void Spawned()
    {
        gameObject.layer = 6;
        m_isDoneSpawning = true;
        m_collider.includeLayers = m_affectedLayer;
    }

    private void FixedUpdate()
    {
        if (m_canMove && m_isDoneSpawning)
        {
            if (SingletonMaster.Instance.PlayerBase != null)
            {
                Vector3 playerPos = SingletonMaster.Instance.PlayerBase.gameObject.transform.position;
                Vector3 myPos = transform.position;
                m_moveDirection = (playerPos - myPos).normalized;
                
                if (m_RB.velocity.magnitude < m_maxSpeed)
                {
                    m_RB.velocity += m_moveDirection * m_acceleration * Time.fixedDeltaTime;
                }
            }
        }
    }

    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_canMove = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player") || other.collider.CompareTag("Enemy"))
        {
            HealthComponent hc = other.gameObject.GetComponent<HealthComponent>();
            if (hc != null)
            {
                hc.DamageEvent.Invoke(m_damage, gameObject);
            }
        }

        if (!other.collider.CompareTag("EnemyProjectile") && m_isDoneSpawning)
        {
            Destroy(gameObject);
        }
    }
}
