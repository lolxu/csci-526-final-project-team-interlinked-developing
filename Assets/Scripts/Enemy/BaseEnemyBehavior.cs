using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class BaseEnemyBehavior : MonoBehaviour
{
    [Header("Basic Settings")] 
    public List<string> m_names = new List<string>();
    public float m_damage = 2.0f;
    
    [Header("AI Settings")]
    public BaseEnemyAI m_AI;
    
    [Header("Visual Settings")] 
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    
    public float m_lootDropRate { get; set; } = 0.0f;
    
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
        
        OnStart();
    }

    private void OnDisable()
    {
        
    }
    
    private void Update()
    {
        OnUpdate();
    }

    protected virtual void OnDamaged(float amount)
    {
        
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

        // if (other.collider.CompareTag("Loot"))
        // {
        //     SingletonMaster.Instance.PlayerBase.RemoveLinkedObject(other.gameObject);
        // }

        if (other.collider.CompareTag("Linkable"))
        {
            
        }
    }
}
