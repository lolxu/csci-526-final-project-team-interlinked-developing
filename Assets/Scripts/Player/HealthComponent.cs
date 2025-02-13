using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")] 
    public bool m_canDamage = true;
    public float m_health = 10.0f;
    public float m_maxHealth = 10.0f;
    public float m_invincibleTime = 1.0f;
    public UnityEvent<float, GameObject> DamageEvent = new UnityEvent<float, GameObject>();
    public UnityEvent<GameObject> DeathEvent = new UnityEvent<GameObject>();
    public bool m_isLinked = false;
    
    // private Sequence m_damageTween = null;
    private Coroutine m_damageSequence = null;
    private bool m_isInvincible = false;
    private Rigidbody2D m_RB;
    private SpriteRenderer m_spriteRenderer;
    private Color m_orgColor;
    private GameObject m_healthBar = null;
    private Vector3 m_orgScale;
    

    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_orgColor = m_spriteRenderer.color;
        m_orgScale = transform.localScale;
        
        DamageEvent.AddListener(OnDamage);
        DeathEvent.AddListener(OnDeath);

        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
        
        // Create Health bar
        m_healthBar = SingletonMaster.Instance.UI.AddHealthBar(this);
    }

    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_isLinked = false;
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_isLinked = true;
        }
    }

    private void OnDisable()
    {
        if (m_healthBar != null)
        {
            Destroy(m_healthBar);
        }
        
        DamageEvent.RemoveListener(OnDamage);
        DeathEvent.RemoveListener(OnDeath);
    }
    
    private void OnDamage(float damage, GameObject instigator)
    {
        if (m_canDamage)
        {
            if (!m_isInvincible && m_health > 0.0f)
            {
                // Only do screen shake on damage to player
                if (gameObject.CompareTag("Player"))
                {
                    Vector2 dir = -(instigator.transform.position - transform.position).normalized;
                    m_damageSequence = StartCoroutine(PlayerHurtSequence(dir));
                    StartCoroutine(PlayerInvincibleSequence());
                    StartCoroutine(PlayerHitStop());
                    SingletonMaster.Instance.CameraShakeManager.Shake(10.0f, 0.25f);
                    
                    m_health -= damage;
                    if (m_health <= 0.0f)
                    {
                        Time.timeScale = 1.0f;
                        StopAllCoroutines();

                        DeathEvent.Invoke(instigator);
                    }
                }
                else if (gameObject.CompareTag("Enemy"))
                {
                    if (m_damageSequence != null)
                    {
                        StopCoroutine(m_damageSequence);
                        m_spriteRenderer.color = m_orgColor;
                        transform.localScale = m_orgScale;
                    }
                    
                    m_damageSequence = StartCoroutine(EnemyHurtSequence());
                    
                    m_health -= damage;
                    if (m_health <= 0.0f)
                    {
                        StopAllCoroutines();
                        RopeComponent rc = GetComponent<RopeComponent>();
                        if (rc != null)
                        {
                            rc.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
                        }
            
                        SingletonMaster.Instance.EventManager.EnemyDeathEvent.Invoke(gameObject);
                    }
                }
            }
        }
    }
    
    private IEnumerator EnemyHurtSequence()
    {
        transform.localScale = m_orgScale * 0.85f;
        m_spriteRenderer.color = Color.white;
        yield return new WaitForSecondsRealtime(0.15f);
        m_spriteRenderer.color = m_orgColor;
        transform.localScale = m_orgScale;
    }
    
    private IEnumerator PlayerHurtSequence(Vector2 dir)
    {
        float flashDuration = 0.0f;
        m_RB.AddForce(dir * 300.0f, ForceMode2D.Impulse);
        while (flashDuration <= m_invincibleTime)
        {
            m_spriteRenderer.color = Color.white;
            yield return new WaitForSecondsRealtime(0.1f);
            m_spriteRenderer.color = m_orgColor;
            yield return new WaitForSecondsRealtime(0.1f);
            flashDuration += 0.2f;
        }
        m_spriteRenderer.color = m_orgColor;
    }

    private void OnDeath(GameObject killer)
    {
        m_isInvincible = false;

        if (gameObject.CompareTag("Player"))
        {
            SingletonMaster.Instance.EventManager.PlayerDeathEvent.Invoke(killer);
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            SingletonMaster.Instance.EventManager.EnemyDeathEvent.Invoke(gameObject);
        }
        else
        {
            SingletonMaster.Instance.PlayerBase.RemoveLinkedObject(gameObject);
        }
        
        Destroy(gameObject);
    }

    private IEnumerator PlayerInvincibleSequence()
    {
        m_isInvincible = true;
        yield return new WaitForSecondsRealtime(m_invincibleTime);
        m_isInvincible = false;
    }

    private IEnumerator PlayerHitStop()
    {
        float orgTimeScale = Time.timeScale;
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = orgTimeScale;
    }
}
