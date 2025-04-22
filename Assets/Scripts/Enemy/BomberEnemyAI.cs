using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BomberEnemyAI : BaseEnemyAI
{
    private enum BomberState
    {
        Attacking,
        MovingToPlayer,
        Idle
    }
    
    [Header("Bomber Settings")] 
    [SerializeField] private float m_bombRange = 5.0f;
    [SerializeField] private float m_bombDamage = 5.0f;
    [SerializeField] private BomberState m_bomberState;
    [SerializeField] private LayerMask m_knockBackMask;
    [SerializeField] private float m_knockBackStrength = 500.0f;
    [SerializeField] private float m_playerKnockbackMult = 75.0f;
    [SerializeField] private float m_telegraphTime = 0.75f;
    [SerializeField] private ParticleSystem m_telegraphParticles;

    private SpriteRenderer m_spRend;
    private Color m_orgColor;
    
    private bool m_canExplode = true;
    private bool m_isExploding = false;
    
    protected override void Start()
    {
        base.Start();

        m_bomberState = BomberState.MovingToPlayer;
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            m_moveTarget = SingletonMaster.Instance.PlayerBase.gameObject;
        }

        m_spRend = GetComponent<SpriteRenderer>();
        m_orgColor = m_spRend.color;
        var main = m_telegraphParticles.main;
        main.duration = m_telegraphTime;
        var shape = m_telegraphParticles.shape;
        shape.radius = m_bombRange;
        
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
        if (obj == gameObject && !m_canExplode)
        {
            m_canExplode = true;
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && m_canExplode)
        {
            m_spRend.DOKill(true);
            m_spRend.color = m_orgColor;
            m_canExplode = false;
        }
    }
    
    protected override void FixedUpdate()
    {
        if (m_moveTarget != null)
        {
            CheckingStates();
            
            Vector3 targetPos = m_moveTarget.transform.position;
            Vector3 faceDir = (targetPos - transform.position).normalized;

            switch (m_bomberState)
            {
                case BomberState.Attacking:
                {
                    BombAttack();
                    break;
                }
                case BomberState.MovingToPlayer:
                {
                    MoveBehavior();
                    break;
                }
                case BomberState.Idle:
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
        if (dist <= m_bombRange && m_bomberState != BomberState.Attacking && m_canExplode)
        {
            m_bomberState = BomberState.Attacking;
        }
        else if (dist > m_bombRange && m_bomberState != BomberState.Attacking)
        {
            if (m_moveTarget == null)
            {
                m_bomberState = BomberState.Idle;
            }
        }
    }

    private void BombAttack()
    {
        if (!m_isExploding && m_canExplode)
        {
            m_isExploding = true;
            
            // Telegraph
            m_telegraphParticles.Play();
            m_spRend.DOColor(Color.white, m_telegraphTime / 6)
                .SetLoops(6, LoopType.Yoyo)
                .SetEase(Ease.InOutFlash)
                .OnComplete(() =>
                {
                    if (m_canExplode)
                    {
                        m_spRend.color = m_orgColor;
                        SingletonMaster.Instance.FeelManager.m_cameraShake.PlayFeedbacks();
                        SingletonMaster.Instance.FeelManager.m_enemyExplode.PlayFeedbacks(transform.position);

                        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, m_bombRange, Vector2.zero,
                            0.0f, m_knockBackMask);
                        foreach (var hit in hits)
                        {
                            if (hit.rigidbody.gameObject != gameObject)
                            {
                                hit.rigidbody.AddExplosionForce(m_knockBackStrength, transform.position, m_bombRange);

                                var hc = hit.rigidbody.gameObject.GetComponent<HealthComponent>();
                                if (hc != null)
                                {
                                    hc.DamageEvent.Invoke(m_bombDamage, gameObject);
                                }

                                if (hit.rigidbody.CompareTag("Player"))
                                {
                                    SingletonMaster.Instance.PlayerBase.StartRagdoll();
                                    hit.rigidbody.AddExplosionForce(m_knockBackStrength * m_playerKnockbackMult,
                                        transform.position, m_bombRange);
                                }
                            }
                        }

                        gameObject.GetComponent<HealthComponent>().DamageEvent.Invoke(1000.0f, gameObject);
                    }
                    else
                    {
                        m_isExploding = false;
                    }
                });
        }
    }

    
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_bombRange);
    }
}
