using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class DangerComponent : MonoBehaviour
{
    [Header("Core Settings")]
    public float m_damage = 2.5f;
    public bool m_oneTimeDamage = false;
    
    [Header("Movement Related")]
    public bool m_canMove = false;
    public float m_moveTime = 2.0f;
    [SerializeField] private Ease m_moveStyle;
    [SerializeField] private GameObject m_end;
    [SerializeField] private LineRenderer m_lineRenderer;
    
    private bool m_canDamage = true;
    private Sequence m_moveSequence;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (m_canDamage)
        {
            if (other.collider.CompareTag("Enemy") || other.collider.CompareTag("Player"))
            {
                other.rigidbody.AddExplosionForce(100.0f, transform.position, 10.0f);
                
                var health = other.gameObject.GetComponent<HealthComponent>();
                if (health != null)
                {
                    health.DamageEvent.Invoke(m_damage, gameObject);
                }
            }
        }

        if (m_oneTimeDamage)
        {
            m_canDamage = false;
        }
    }


    private void Start()
    {
        if (m_canMove && m_end != null && m_lineRenderer != null)
        {
            m_lineRenderer.SetPosition(1, m_end.transform.localPosition);
            m_moveSequence = DOTween.Sequence();
            TweenMove();
        }
        else if (m_canMove)
        {
            Debug.LogError("The danger can move, but is not setup correctly");
        }
    }

    private void OnDrawGizmos()
    {
        if (m_canMove && m_end != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, m_end.transform.position);
            Gizmos.DrawWireSphere(m_end.transform.position, 0.5f);
        }
    }

    private void TweenMove()
    {
        m_moveSequence.Append(m_lineRenderer
            .DOColor(new Color2(m_lineRenderer.startColor, m_lineRenderer.endColor),
                new Color2(Color.white, Color.white), 0.25f)
            .SetLoops(3, LoopType.Restart)
            .OnComplete(() =>
            {
                m_lineRenderer.enabled = false;
            }));
        m_moveSequence.Append(transform
            .DOMove(m_end.transform.position, m_moveTime)
            .SetEase(m_moveStyle)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            }));
    }

    // Use trigger for moving danger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_canDamage)
        {
            if (m_canDamage)
            {
                if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Player"))
                {
                    var health = other.gameObject.GetComponent<HealthComponent>();
                    if (health != null)
                    {
                        health.DamageEvent.Invoke(m_damage, gameObject);
                    }
                }
            }
        }

        if (m_oneTimeDamage)
        {
            m_canDamage = false;
        }
    }
}
