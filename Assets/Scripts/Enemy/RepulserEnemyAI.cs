using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RepulserEnemyAI : BaseEnemyAI
{
    private enum RepulserState
    {
        Attacking,
        MovingToPlayer,
        Idle
    }
    
    [Header("Repulser Settings")] 
    [SerializeField] private float m_repulseTimeout = 2.0f;
    [SerializeField] private float m_repulseDuration = 0.25f;
    [SerializeField] private float m_repulseRange = 5.0f;
    [SerializeField] private RepulserState m_repulserState;
    [SerializeField] private LayerMask m_knockBackMask;
    [SerializeField] private float m_knockBackStrength = 500.0f;
    [SerializeField] private float m_playerKnockbackMult = 75.0f;

    private SpriteRenderer m_spRend;
    
    private bool m_repulseCooled = true;
    private bool m_canRepulse = true;
    private bool m_isRepulsing = false;
    
    private Coroutine m_cooldownRoutine = null;
    
    protected override void Start()
    {
        base.Start();

        m_repulserState = RepulserState.MovingToPlayer;
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            m_moveTarget = SingletonMaster.Instance.PlayerBase.gameObject;
        }

        m_spRend = GetComponent<SpriteRenderer>();
        
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
    }
    
    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && !m_canRepulse)
        {
            m_canRepulse = true;
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && m_canRepulse)
        {
            m_canRepulse = false;
        }
    }
    
    protected override void FixedUpdate()
    {
        if (m_moveTarget != null)
        {
            CheckingStates();
            
            Vector3 targetPos = m_moveTarget.transform.position;
            Vector3 faceDir = (targetPos - transform.position).normalized;

            switch (m_repulserState)
            {
                case RepulserState.Attacking:
                {
                    RepulserAttack();
                    break;
                }
                case RepulserState.MovingToPlayer:
                {
                    MoveBehavior();
                    break;
                }
                case RepulserState.Idle:
                {
                    break;
                }
            }
            
            // moving face
            m_face.transform.localPosition = faceDir * m_faceMoveFactor;
        }
    }
    
    private void CheckingStates()
    {
        float dist = Vector3.Distance(transform.position, m_moveTarget.transform.position);
        if (dist <= m_repulseRange && m_repulserState != RepulserState.Attacking && m_repulseCooled)
        {
            m_repulserState = RepulserState.Attacking;
        }
        else if (dist > m_repulseRange)
        {
            if (m_moveTarget == null)
            {
                m_repulserState = RepulserState.Idle;
            }
            else if (!m_isRepulsing)
            {
                if (m_cooldownRoutine != null)
                {
                    StopCoroutine(m_cooldownRoutine);
                    m_repulseCooled = true;
                    m_repulserState = RepulserState.MovingToPlayer;
                }
            }
        }
    }

    private void RepulserAttack()
    {
        if (!m_isRepulsing && m_repulseCooled && m_canRepulse)
        {
            m_repulseCooled = false;
            m_isRepulsing = true;
            
            GetComponent<HealthComponent>().m_canDamage = false;
            
            // Telegraph
            Color orgColor = m_spRend.color;
            m_spRend.DOColor(Color.white, 0.05f)
                .SetLoops(6, LoopType.Yoyo)
                .SetEase(Ease.InOutFlash)
                .OnComplete(() =>
                {
                    m_spRend.color = orgColor;
                    if (m_canRepulse)
                    {
                        StartCoroutine(Repulse());
                    }
                    else
                    {
                        m_isRepulsing = false;
                        m_repulseCooled = true;
                        GetComponent<HealthComponent>().m_canDamage = true;
                    }
                });
        }
    }

    private IEnumerator Repulse()
    {
        Vector3 orgScale = transform.localScale;
        transform.DOScale(orgScale * 2.0f, 0.1f).SetEase(Ease.InOutExpo);
        
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, m_repulseRange, Vector2.zero, 0.0f, m_knockBackMask);
        foreach (var hit in hits)
        {
            if (hit.rigidbody.gameObject != gameObject)
            {
                hit.rigidbody.AddExplosionForce(m_knockBackStrength, transform.position, m_repulseRange);

                if (hit.rigidbody.CompareTag("Player"))
                {
                    SingletonMaster.Instance.PlayerBase.StartRagdoll();
                    hit.rigidbody.AddExplosionForce(m_knockBackStrength * m_playerKnockbackMult, transform.position, m_repulseRange);
                }
            }
        }
        
        yield return new WaitForSeconds(m_repulseDuration);
        transform.DOScale(orgScale, 0.1f).SetEase(Ease.InOutExpo);
        
        // Resetting things
        m_isRepulsing = false;
        if (m_moveTarget != null)
        {
            m_repulserState = RepulserState.MovingToPlayer;
        }
        else
        {
            m_repulserState = RepulserState.Idle;
        }
        GetComponent<HealthComponent>().m_canDamage = true;

        m_cooldownRoutine = StartCoroutine(RepulseCooldown());
    }
    
    private IEnumerator RepulseCooldown()
    {
        yield return new WaitForSeconds(m_repulseTimeout);
        m_repulseCooled = true;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_repulseRange);
    }
}
