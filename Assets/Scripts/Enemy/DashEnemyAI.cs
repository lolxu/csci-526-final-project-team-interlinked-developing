using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class DashEnemyAI : BaseEnemyAI
{
    private enum DasherState
    {
        Attacking,
        MovingToPlayer,
        Idle
    }

    [Header("Dasher Settings")] 
    [SerializeField] private float m_dashTimeout = 2.0f;
    [SerializeField] private float m_dashDuration = 0.25f;
    [SerializeField] private float m_dashRange = 5.0f;
    [SerializeField] private DasherState m_dasherState;
    [SerializeField] private LineRenderer m_telegraph;
    [SerializeField] private float m_dashMult = 5.0f;

    private bool m_isDashing = false;
    private bool m_dashCooled = true;
    private bool m_canDash = true;

    private Coroutine m_cooldownRoutine = null;
    
    protected override void Start()
    {
        base.Start();

        m_dasherState = DasherState.MovingToPlayer;
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            m_moveTarget = SingletonMaster.Instance.PlayerBase.gameObject;
        }
        
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
        if (obj == gameObject && !m_canDash)
        {
            m_canDash = true;
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && m_canDash)
        {
            m_canDash = false;
        }
    }
    
    protected override void FixedUpdate()
    {
        if (m_moveTarget != null)
        {
            CheckingStates();
            
            Vector3 targetPos = m_moveTarget.transform.position;
            Vector3 faceDir = (targetPos - transform.position).normalized;

            switch (m_dasherState)
            {
                case DasherState.Attacking:
                {
                    DashAttack();
                    break;
                }
                case DasherState.MovingToPlayer:
                {
                    MoveBehavior();
                    break;
                }
                case DasherState.Idle:
                {
                    break;
                }
            }

            if (m_willTurnToPlayer)
            {
                if (m_moveTarget != null && !m_isDashing && !GetComponent<Collider2D>().isTrigger)
                {
                    Vector3 toTarget = (m_moveTarget.transform.position - transform.position).normalized;
                    Quaternion targetQuat = Quaternion.LookRotation(toTarget, Vector3.forward);
                    Quaternion rotQuat = Quaternion.Slerp(transform.rotation, targetQuat, Time.deltaTime * 10.0f);
                    m_RB.MoveRotation(rotQuat);
                    
                    faceDir = transform.InverseTransformDirection(faceDir);
                }
            }
            
            // moving face
            m_face.transform.localPosition = faceDir * m_faceMoveFactor;
            
        }
    }

    private void DashAttack()
    {
        if (!m_isDashing && m_dashCooled && m_canDash)
        {
            m_dashCooled = false;
            m_isDashing = true;
            
            GetComponent<HealthComponent>().m_canDamage = false;
            
            m_telegraph.enabled = true;
            m_telegraph.SetPosition(0, m_telegraph.transform.InverseTransformPoint(transform.position));
            m_telegraph.SetPosition(1, m_telegraph.transform.InverseTransformPoint(m_moveTarget.transform.position));

            Vector3 dashDir = (m_moveTarget.transform.position - transform.position).normalized;

            Color orgStart = m_telegraph.startColor;
            Color orgEnd = m_telegraph.endColor;

            m_telegraph
                .DOColor(new Color2(m_telegraph.startColor, m_telegraph.endColor),
                    new Color2(Color.white, Color.clear), 0.25f)
                .SetLoops(3, LoopType.Restart)
                .OnComplete(() =>
                {
                    m_telegraph.startColor = orgStart;
                    m_telegraph.endColor = orgEnd;
                    
                    m_telegraph.enabled = false;

                    if (m_canDash)
                    {
                        StartCoroutine(Dash(dashDir));
                    }
                    else
                    {
                        m_isDashing = false;
                        m_dashCooled = true;
                        GetComponent<HealthComponent>().m_canDamage = true;
                    }
                });
        }
    }

    private IEnumerator Dash(Vector3 direction)
    {
        float timer = 0.0f;
        while (timer < m_dashDuration)
        {
            timer += Time.deltaTime;
            GetComponent<Rigidbody2D>().velocity = direction * m_dashMult;
            yield return null;
        }

        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        m_isDashing = false;

        if (m_moveTarget != null)
        {
            m_dasherState = DasherState.MovingToPlayer;
        }
        else
        {
            m_dasherState = DasherState.Idle;
        }
        
        GetComponent<HealthComponent>().m_canDamage = true;

        m_cooldownRoutine = StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(m_dashTimeout);
        m_dashCooled = true;
    }

    private void CheckingStates()
    {
        float dist = Vector3.Distance(transform.position, m_moveTarget.transform.position);
        if (dist <= m_dashRange && m_dasherState != DasherState.Attacking && m_dashCooled)
        {
            m_dasherState = DasherState.Attacking;
        }
        else if (dist > m_dashRange)
        {
            if (m_moveTarget == null)
            {
                m_dasherState = DasherState.Idle;
            }
            else if (!m_isDashing)
            {
                if (m_cooldownRoutine != null)
                {
                    StopCoroutine(m_cooldownRoutine);
                    m_dashCooled = true;
                    m_dasherState = DasherState.MovingToPlayer;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_dashRange);
    }
}
