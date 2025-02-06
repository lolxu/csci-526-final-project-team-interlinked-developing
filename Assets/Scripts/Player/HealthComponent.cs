using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")] 
    public float m_health = 10.0f;
    public float m_maxHealth = 10.0f;
    public float m_invincibleTime = 1.0f;
    public UnityEvent<float, GameObject> DamageEvent = new UnityEvent<float, GameObject>();
    public UnityEvent<GameObject> DeathEvent = new UnityEvent<GameObject>();
    
    // private Sequence m_damageTween = null;
    private Coroutine m_damageSequence = null;
    private bool m_isInvincible = false;
    private Rigidbody2D m_RB;
    private SpriteRenderer m_spriteRenderer;
    private Color m_orgColor;
    private GameObject m_healthBar = null;

    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_orgColor = m_spriteRenderer.color;
        
        DamageEvent.AddListener(OnDamage);
        DeathEvent.AddListener(OnDeath);
        
        // Create Health bar
        m_healthBar = SingletonMaster.Instance.UI.AddHealthBar(this);
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
        if (!m_isInvincible && m_health > 0.0f)
        {
            // Do some juice stuff here
            // if (m_damageTween != null)
            // {
            //     m_damageTween.Kill(true);
            // }
            
            if (m_damageSequence != null)
            {
                // StopCoroutine(m_damageSequence);
                // m_spriteRenderer.color = m_orgColor;
                // transform.localScale = m_orgScale;
            }

            Vector2 dir = -(instigator.transform.position - transform.position).normalized;
            m_damageSequence = StartCoroutine(HurtSequence(dir));

            StartCoroutine(InvincibleSequence());
            StartCoroutine(HitStop());
            
            // Juice Stuff
            // SingletonMaster.Instance.FeelManager.m_cameraShake.PlayFeedbacks(Vector3.zero, 2.5f);
            SingletonMaster.Instance.CameraShakeManager.Shake(10.0f, 0.25f);
            m_health -= damage;
            if (m_health <= 0.0f)
            {
                Time.timeScale = 1.0f;
                StopAllCoroutines();
                // m_damageTween.Kill();
                // m_damageTween = DOTween.Sequence();
                // m_damageTween.Insert(0, m_spriteRenderer.DOColor(Color.white, 0.1f)
                //     .SetLoops(1, LoopType.Yoyo)
                //     .SetEase(Ease.InOutFlash));
                // m_damageTween.Insert(0,
                //     transform.DOPunchScale(transform.localScale * 0.5f, 0.1f));
                // m_damageTween.OnComplete(() =>
                // {
                //     SingletonMaster.Instance.EventManager.PlayerDeathEvent.Invoke(instigator);
                // });
                
                DeathEvent.Invoke(instigator);
            }
            else
            {
                // m_damageTween = DOTween.Sequence();
                // m_damageTween.Insert(0, m_spriteRenderer.DOColor(Color.white, 0.1f)
                //     .SetLoops((int)(m_invincibleTime / 0.1f), LoopType.Yoyo)
                //     .SetEase(Ease.InOutFlash).OnComplete(() => { m_spriteRenderer.color = m_orgColor; }));
                // m_damageTween.Insert(0,
                //     transform.DOPunchScale(transform.localScale * 0.5f, 0.1f).OnComplete(() =>
                //     {
                //         transform.localScale = m_orgScale;
                //     }));
            }
        }
    }
    
    private IEnumerator HurtSequence(Vector2 dir)
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

        if (gameObject.CompareTag("Linkable"))
        {
            SingletonMaster.Instance.PlayerBase.RemoveLinkedObject(gameObject);
        }
        
        Destroy(gameObject);
    }

    private IEnumerator InvincibleSequence()
    {
        m_isInvincible = true;
        yield return new WaitForSecondsRealtime(m_invincibleTime);
        m_isInvincible = false;
    }

    private IEnumerator HitStop()
    {
        float orgTimeScale = Time.timeScale;
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = orgTimeScale;
    }
}
