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

    [Header("Visual Settings")] 
    [SerializeField] private GameObject m_playerDeath;
    [SerializeField] private GameObject m_enemyDeath;
    
    private Coroutine m_damageSequence = null;
    private bool m_isInvincible = false;
    private Rigidbody2D m_RB;
    private SpriteRenderer m_spriteRenderer;
    private Color m_orgColor;
    private GameObject m_healthBar = null;
    private Vector3 m_orgScale;

    // private Sequence m_hurtSequence = null;

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
    
    // TODO: Change magic numbers...
    private void OnDamage(float damage, GameObject instigator)
    {
        if (m_canDamage)
        {
            if (!m_isInvincible && m_health > 0.0f)
            {
                // First stop the sequence & resetting stuff
                // m_hurtSequence.Kill();
                m_spriteRenderer.color = m_orgColor;
                transform.localScale = m_orgScale;
                
                // For player hurting
                if (gameObject.CompareTag("Player"))
                {
                    Vector2 dir = -(instigator.transform.position - transform.position).normalized;
                    
                    // Add a small force
                    // m_RB.AddForce(dir * 300.0f, ForceMode2D.Impulse);
                    m_damageSequence = StartCoroutine(PlayerHurtSequence(dir));
                    StartCoroutine(PlayerInvincibleSequence());
                    StartCoroutine(PlayerHitStop());
                    
                    // TODO: Player Hurt Sequence - Used Later...
                    // m_isInvincible = true;
                    // m_hurtSequence = DOTween.Sequence();
                    // m_hurtSequence.Insert(0,
                    //     m_spriteRenderer.DOColor(Color.white, m_invincibleTime / 6)
                    //     .SetLoops(6, LoopType.Yoyo)
                    //     .SetEase(Ease.InOutBounce));
                    // m_hurtSequence.Insert(0,
                    //     transform.DOPunchScale(m_orgScale * 0.85f, 0.15f));
                    // m_hurtSequence.OnComplete(() =>
                    // {
                    //     m_isInvincible = false;
                    //     m_spriteRenderer.color = m_orgColor;
                    //     transform.localScale = m_orgScale;
                    // });
                    
                    // Do a camera shake
                    SingletonMaster.Instance.CameraShakeManager.Shake(10.0f, 0.25f);
                    // SingletonMaster.Instance.FeelManager.m_cameraShake.PlayFeedbacks(Vector3.zero, 1.0f);
                    
                    // Damage to health
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
                    // TODO: Enemy Hurt Sequence
                    // m_hurtSequence = DOTween.Sequence();
                    // m_hurtSequence.Insert(0,
                    //     m_spriteRenderer.DOColor(Color.white, 0.15f)
                    //         .SetLoops(2, LoopType.Yoyo)
                    //         .SetEase(Ease.InOutBounce));
                    // m_hurtSequence.Insert(0,
                    //     transform.DOPunchScale(m_orgScale * 0.85f, 0.15f));
                    // m_hurtSequence.OnComplete(() =>
                    // {
                    //     m_spriteRenderer.color = m_orgColor;
                    //     transform.localScale = m_orgScale;
                    // });

                    if (m_damageSequence != null)
                    {
                        StopCoroutine(m_damageSequence);
                        m_spriteRenderer.color = m_orgColor;
                        transform.localScale = m_orgScale;
                    }
                    
                    m_damageSequence = StartCoroutine(EnemyHurtSequence());
                    
                    // Damage to health
                    m_health -= damage;
                    if (m_health <= 0.0f)
                    {
                        // TODO: Force kill the sequence
                        // m_hurtSequence.Kill();
                        
                        RopeComponent rc = GetComponent<RopeComponent>();
                        if (rc != null && SingletonMaster.Instance.PlayerBase != null)
                        {
                            rc.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
                        }
                        DeathEvent.Invoke(gameObject);
                    }
                }
            }
        }
    }

    private void OnDeath(GameObject killer)
    {
        if (gameObject.CompareTag("Player"))
        {
            m_isInvincible = false;
            // SingletonMaster.Instance.FeelManager.m_playerDeath.PlayFeedbacks(transform.position);
            Instantiate(m_playerDeath, transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
            SingletonMaster.Instance.EventManager.PlayerDeathEvent.Invoke(killer);
            Destroy(gameObject);
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            Instantiate(m_enemyDeath, transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
            SingletonMaster.Instance.EventManager.EnemyDeathEvent.Invoke(gameObject);
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
    
    private IEnumerator PlayerInvincibleSequence()
    {
        m_isInvincible = true;
        yield return new WaitForSecondsRealtime(m_invincibleTime);
        m_isInvincible = false;
    }
}
