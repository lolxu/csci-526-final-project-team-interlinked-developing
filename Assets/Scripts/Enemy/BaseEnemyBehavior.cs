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
    public float m_health = 10.0f;
    public float m_damage = 2.0f;
    
    [Header("AI Settings")]
    public BaseEnemyAI m_AI;
    
    [Header("Visual Settings")] 
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    
    [Header("Enemy Events")]
    public UnityEvent<float> EnemyDamagedEvent = new UnityEvent<float>();
    
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
        
        EnemyDamagedEvent.AddListener(OnDamaged);
        
        OnStart();
    }

    private void OnDisable()
    {
        EnemyDamagedEvent.RemoveListener(OnDamaged);
    }

    private void FixedUpdate()
    {
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            Vector3 toActualPlayer = (SingletonMaster.Instance.PlayerBase.transform.position - transform.position)
                .normalized;
            
        }
    }

    private void Update()
    {
        OnUpdate();
    }

    protected virtual void OnDamaged(float amount)
    {
        // Kills tween if still playing
        // if (m_damageTween != null)
        // {
        //     m_damageTween.Kill(true);
        // }
        
        // Juice Tweens
        // m_damageTween = DOTween.Sequence();
        // m_damageTween.Insert(0, m_spriteRenderer.DOColor(Color.white, 0.1f).SetLoops(1, LoopType.Yoyo)
        //     .SetEase(Ease.InOutFlash).OnComplete(() =>
        //     {
        //         m_spriteRenderer.color = m_orgColor;
        //     }));
        // m_damageTween.Insert(0, transform.DOPunchScale(transform.localScale * 0.5f, 0.1f).OnComplete(() =>
        // {
        //     transform.localScale = m_orgScale;
        // }));
        // m_damageTween.OnComplete(() =>
        // {
        //     m_health -= amount;
        //     if (m_health < 0.0f)
        //     {
        //         SingletonMaster.Instance.EventManager.EnemyDeathEvent.Invoke(gameObject);
        //     }
        // });
        if (m_damageSequence != null)
        {
            StopCoroutine(m_damageSequence);
            m_spriteRenderer.color = m_orgColor;
            transform.localScale = m_orgScale;
        }
        m_damageSequence = StartCoroutine(EnemyHurtSequence());
        
        m_health -= amount;
        if (m_health < 0.0f)
        {
            SingletonMaster.Instance.EventManager.EnemyDeathEvent.Invoke(gameObject);
        }
        
        Debug.Log(gameObject + " enemy damaged");
    }

    private IEnumerator EnemyHurtSequence()
    {
        transform.localScale = m_orgScale * 0.85f;
        m_spriteRenderer.color = Color.white;
        yield return new WaitForSecondsRealtime(0.15f);
        m_spriteRenderer.color = m_orgColor;
        transform.localScale = m_orgScale;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            PlayerBase player = other.gameObject.GetComponent<PlayerBase>();
            if (player != null)
            {
                player.m_healthComponent.DamageEvent.Invoke(m_damage, gameObject);
            }
        }

        if (other.collider.CompareTag("Loot"))
        {
            SingletonMaster.Instance.PlayerBase.RemoveLinkedObject(other.gameObject);
        }

        if (other.collider.CompareTag("Linkable"))
        {
            HealthComponent health = other.gameObject.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.DamageEvent.Invoke(m_damage, gameObject);
            }
        }
    }
}
