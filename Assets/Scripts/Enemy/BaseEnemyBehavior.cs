using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utility;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class BaseEnemyBehavior : MonoBehaviour
{
    [Header("Basic Settings")] 
    public List<string> m_names = new List<string>();
    public HealthComponent m_healthComponent;
    public float m_damage = 2.0f;
    public float m_collisionVelocityThreshold = 10.0f;
    
    [Header("AI Settings")]
    public BaseEnemyAI m_AI;
    
    [Header("Visual Settings")] 
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    
    public float m_lootDropRate { get; set; } = 0.0f;

    private bool m_canBeTossed = false;
    private Color m_orgColor;
    private Vector3 m_orgScale;
    
    // private Sequence m_damageTween = null;
    private Coroutine m_damageSequence = null;
    
    /// <summary>
    /// Overwrite this for custom start behavior
    /// </summary>
    protected virtual void OnStart() { }
    
    /// <summary>
    /// Overwrite this for custom update behavior
    /// </summary>
    protected virtual void OnUpdate() { }

    private void Start()
    {
        m_orgColor = m_spriteRenderer.color;
        m_orgScale = transform.localScale;
        
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
        
        OnStart();
    }
    
    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
    }

    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && instigator.CompareTag("Player"))
        {
            m_canBeTossed = false;
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && instigator.CompareTag("Player"))
        {
            m_canBeTossed = true;
        }
    }
    
    private void Update()
    {
        OnUpdate();
    }

    protected virtual void OnDamaged(float amount)
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Checking colliding force
        if (!other.gameObject.CompareTag("Rope") && m_canBeTossed)
        {
            float relativeVel = other.relativeVelocity.magnitude;
            if (relativeVel > m_collisionVelocityThreshold)
            {
                // Remap relative velocity magnitude to health
                relativeVel = Mathf.Clamp(relativeVel, 0.0f, m_collisionVelocityThreshold * 1.5f);
                relativeVel = relativeVel.Remap(m_collisionVelocityThreshold, m_collisionVelocityThreshold * 1.5f, 0.0f, m_healthComponent.m_maxHealth * 0.3f);
                
                Debug.Log("Damage: " + relativeVel);
                m_healthComponent.DamageEvent.Invoke(relativeVel, gameObject);

                if (other.gameObject.CompareTag("Enemy"))
                {
                    HealthComponent health = other.gameObject.GetComponent<HealthComponent>();
                    if (health != null)
                    {
                        health.DamageEvent.Invoke(relativeVel, gameObject);
                    }
                }
            }
        }
        
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            HealthComponent health = other.gameObject.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.DamageEvent.Invoke(m_damage, gameObject);
            }
        }
    }
}
