using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public List<GameObject> m_pathObjects = new List<GameObject>();
    [SerializeField] private GameObject m_telegraphEnd;
    [SerializeField] private LineRenderer m_lineRenderer;

    private Vector3 m_orgPosition;
    private bool m_canDamage = true;
    private Sequence m_moveSequence;

    private void Start()
    {
        m_orgPosition = transform.position;
        if (m_canMove && m_telegraphEnd != null && m_lineRenderer != null)
        {
            Vector3 telegraphEndPos = m_telegraphEnd.transform.localPosition;
            m_lineRenderer.SetPosition(1, telegraphEndPos);
            m_moveSequence = DOTween.Sequence();
            TweenMove();
        }
        else if (m_canMove)
        {
            Debug.LogError("The danger can move, but is not setup correctly");
        }
    }

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

    private void OnDrawGizmos()
    {
        if (m_canMove && m_telegraphEnd != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, m_telegraphEnd.transform.position);
            Gizmos.DrawWireSphere(m_telegraphEnd.transform.position, 0.5f);
        }
    }

    private void TweenMove()
    {
        Color orgStartColor = m_lineRenderer.startColor;
        Color orgEndColor = m_lineRenderer.endColor;
        m_moveSequence.Append(m_lineRenderer
            .DOColor(new Color2(m_lineRenderer.startColor, m_lineRenderer.endColor),
                new Color2(Color.white, Color.white), 0.25f)
            .SetLoops(3, LoopType.Restart)
            .OnComplete(() =>
            {
                m_lineRenderer.startColor = orgStartColor;
                m_lineRenderer.endColor = orgEndColor;
                m_lineRenderer.enabled = false;
            }));

        float pathTime = m_moveTime / m_pathObjects.Count;
        foreach (var pathObject in m_pathObjects)
        {
            Vector3 pathPos = pathObject.transform.position;
            float heightOffset = transform.localScale.y / 2.0f;

            Vector3 adjustedPos = pathPos;
            adjustedPos -= heightOffset * transform.up;
            
            Debug.Log(adjustedPos);

            m_moveSequence.Append(transform
                .DOMove(adjustedPos, pathTime)
                .SetEase(m_moveStyle));
        }
        
        m_moveSequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
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
