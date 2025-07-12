using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")] 
    public bool m_canDamage = true;
    public float m_health = 10.0f;
    public float m_maxHealth = 10.0f;
    public float m_invincibleTime = 1.0f;
    public float m_cooldownRecoverRate = 1.0f;
    public UnityEvent<float, GameObject> DamageEvent = new UnityEvent<float, GameObject>();
    public UnityEvent<GameObject> DeathEvent = new UnityEvent<GameObject>();
    public bool m_isLinked = false;

    [Header("Heal Settings")] 
    public bool m_isHealing = false;
    public GameObject m_healer = null;

    [Header("Visual Settings")] 
    [SerializeField] private GameObject m_playerDeath;
    [SerializeField] private GameObject m_enemyDeath;
    
    // private Coroutine m_damageSequence = null;
    private bool m_isInvincible = false;
    private Rigidbody2D m_RB;
    private SpriteRenderer m_spriteRenderer;
    private Color m_orgColor;
    private GameObject m_healthBar = null;
    private Vector3 m_orgScale;

    private Sequence m_hurtSequence = null;

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
        
        SingletonMaster.Instance.EventManager.CooldownStarted.AddListener(OnCooldownStarted);
        
        // Create Health bar
        m_healthBar = SingletonMaster.Instance.UI.AddHealthBar(this);
    }

    public void SetHealthBarOffset(float offset)
    {
        if (m_healthBar != null)
        {
            m_healthBar.GetComponent<HealthBar>().SetYOffset(offset);
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
        
        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
        
        SingletonMaster.Instance.EventManager.CooldownStarted.RemoveListener(OnCooldownStarted);
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
    
    private void OnCooldownStarted(float duration)
    {
        if (gameObject.CompareTag("Player"))
        {
            StartCoroutine(CooldownHeal(duration));
        }
    }

    private IEnumerator CooldownHeal(float duration)
    {
        float timer = 0.0f;
        while (timer <= duration)
        {
            timer += Time.deltaTime;
            m_health += m_cooldownRecoverRate * Time.deltaTime;
            if (m_health > m_maxHealth)
            {
                m_health = m_maxHealth;
            }
            yield return null;
        }
    }
    
    // TODO: Change magic numbers...
    private void OnDamage(float damage, GameObject instigator)
    {
        if (m_canDamage)
        {
            if (!m_isInvincible && m_health > 0.0f)
            {
                // First stop the sequence & resetting stuff
                m_hurtSequence.Kill();
                m_spriteRenderer.color = m_orgColor;
                transform.localScale = m_orgScale;
                
                // For player hurting
                if (gameObject.CompareTag("Player"))
                {
                    Vector2 dir = -(instigator.transform.position - transform.position).normalized;
                    
                    // Add a small force
                    m_RB.AddForce(dir * 300.0f, ForceMode2D.Impulse);
                    // m_damageSequence = StartCoroutine(PlayerHurtSequence(dir));
                    StartCoroutine(PlayerInvincibleSequence());
                    StartCoroutine(PlayerHitStop());
                    
                    // Player Hurt Sequence - DOTween
                    m_isInvincible = true;
                    m_hurtSequence = DOTween.Sequence();
                    m_hurtSequence.Insert(0,
                        m_spriteRenderer.DOColor(Color.white, m_invincibleTime / 6)
                        .SetLoops(6, LoopType.Yoyo)
                        .SetEase(Ease.InOutBounce));
                    // m_hurtSequence.Insert(0,
                    //     transform.DOPunchScale(m_orgScale * 0.85f, 0.15f));
                    m_hurtSequence.OnComplete(() =>
                    {
                        m_isInvincible = false;
                        m_spriteRenderer.color = m_orgColor;
                        transform.localScale = m_orgScale;
                    });
                    
                    // Do a camera shake
                    // SingletonMaster.Instance.CameraShakeManager.Shake(10.0f, 0.25f);
                    SingletonMaster.Instance.FeelManager.m_cameraShake.PlayFeedbacks(transform.position, 1.0f);
                    
                    // Damage to health
                    m_health -= damage;
                    if (m_health <= 0.0f)
                    {
                        Time.timeScale = 1.0f;
                        StopAllCoroutines();
                        DeathEvent.Invoke(instigator);
                    }
                    
                    // Play Player Hit sfx
                    SingletonMaster.Instance.AudioManager.PlayOtherSFX("PlayerHit");
                }
                else if (gameObject.CompareTag("Enemy"))
                {
                    // Enemy Hurt Sequence - DOTween
                    m_hurtSequence = DOTween.Sequence();
                    m_hurtSequence.Insert(0,
                        m_spriteRenderer.DOColor(Color.white, 0.15f)
                            .SetLoops(2, LoopType.Yoyo)
                            .SetEase(Ease.InOutBounce));
                    m_hurtSequence.Insert(0,
                        transform.DOPunchScale(m_orgScale * 0.85f, 0.15f));
                    m_hurtSequence.OnComplete(() =>
                    {
                        m_spriteRenderer.color = m_orgColor;
                        transform.localScale = m_orgScale;
                    });
                    
                    // Damage to health
                    m_health -= damage;
                    SingletonMaster.Instance.EventManager.EnemyDamagedEvent.Invoke(gameObject);
                    
                    if (m_health <= 0.0f)
                    {
                        // Force kill the sequence
                        m_hurtSequence.Kill();
                        
                        RopeComponent rc = GetComponent<RopeComponent>();
                        if (rc != null && SingletonMaster.Instance.PlayerBase != null)
                        {
                            rc.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
                        }
                        DeathEvent.Invoke(gameObject);
                    }
                    
                    // Play Enemy Hit sfx
                    SingletonMaster.Instance.AudioManager.PlayOtherSFX("EnemyHit");
                }
            }
        }
    }

    private void OnDeath(GameObject killer)
    {
        if (gameObject.CompareTag("Player"))
        {
            m_isInvincible = false;
            SingletonMaster.Instance.FeelManager.m_playerDeath.PlayFeedbacks(transform.position);
            SingletonMaster.Instance.EventManager.PlayerDeathEvent.Invoke(killer);
            
            // Metrics
            int level = SceneManager.GetActiveScene().buildIndex;
            
            Vector2 deathPos = transform.position;
            MetricsManager.Instance.m_metricsData.RecordDeath(level, deathPos);
            
            // Rope Disconnect Audio
            SingletonMaster.Instance.AudioManager.PlayPlayerSFX("PlayerDeath");
            
            Destroy(gameObject);
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            SingletonMaster.Instance.FeelManager.m_enemyDeath.PlayFeedbacks(transform.position);
            SingletonMaster.Instance.EventManager.EnemyDeathEvent.Invoke(gameObject);
            
            SingletonMaster.Instance.AudioManager.PlayOtherSFX("EnemyDeath"); // Free's sound
        }
        else
        {
            SingletonMaster.Instance.PlayerBase.RemoveLinkedObject(gameObject);
        }
    }
    
    private IEnumerator PlayerHitStop()
    {
        float orgTimeScale = Time.timeScale;
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = orgTimeScale;
    }
    
    
    private IEnumerator PlayerInvincibleSequence()
    {
        m_isInvincible = true;
        yield return new WaitForSecondsRealtime(m_invincibleTime);
        m_isInvincible = false;
    }
}
