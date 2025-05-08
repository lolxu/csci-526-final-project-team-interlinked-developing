using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// TODO: Refactor this into a loot class
public class HealthPickup : MonoBehaviour
{
    
    [Header("Health pickup settings")]
    public float m_healValue = 10.0f;
    public float m_maxValue = 10.0f;
    public bool m_canDespawn = true;
    [SerializeField] private float m_healRate = 1.0f;
    [SerializeField] private RopeComponent m_ropeComponent;
    [SerializeField] private float m_lifeTime = 20.0f;
    
    [Header("Visual Settings")]
    [SerializeField] private float m_scaleFactor = 1.5f;
    [SerializeField] private AnimationCurve m_curve;
    [SerializeField] private float m_shrinkTime = 0.15f;
    [SerializeField] private List<Color> m_collectedColor = new List<Color>();
    [SerializeField] private List<SpriteRenderer> m_spriteRenderers = new List<SpriteRenderer>();
    
    [Header("Damage Settings")]
    [SerializeField] private float m_damage = 1.5f;
    [SerializeField] private float m_velocityThreshold = 10.0f;

    private List<Color> m_uncollectedColor = new List<Color>();
    private Coroutine shrinkCoroutine = null;
    private float m_despawnTimer = 0.0f;
    
    private bool m_isHealing = false;
    private bool m_isDespawning = false;

    private void Awake()
    {
        m_healValue = m_maxValue;

        foreach (var sp in m_spriteRenderers)
        {
            m_uncollectedColor.Add(sp.color);
        }
    }

    private void Start()
    {
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
    }

    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && instigator.CompareTag("Player"))
        {
            m_isHealing = false;
            for (int i = 0; i < m_spriteRenderers.Count; i++)
            {
                m_spriteRenderers[i].color = m_uncollectedColor[i];
            }
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && instigator.CompareTag("Player"))
        {
            m_isHealing = true;
            for (int i = 0; i < m_spriteRenderers.Count; i++)
            {
                m_spriteRenderers[i].color = m_collectedColor[i];
            }
            
            m_despawnTimer = 0.0f;
        }
    }

    private void Update()
    {
        if (!m_isDespawning && SingletonMaster.Instance.PlayerBase != null)
        {
            if (m_isHealing)
            {
                m_despawnTimer = 0.0f;
                var health = SingletonMaster.Instance.PlayerBase.GetComponent<HealthComponent>();
                if (health)
                {
                    if (health.m_health < health.m_maxHealth)
                    {
                        health.m_health += m_healRate * Time.deltaTime;
                        m_healValue -= m_healRate * Time.deltaTime;
                        float scale = m_curve.Evaluate((float)m_healValue / m_maxValue) * m_scaleFactor;
                        transform.localScale = Vector3.one * scale;

                        if (m_healValue <= 0.0f)
                        {
                            m_healValue = 0.0f;
                            m_ropeComponent.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
                            m_isDespawning = true;
                            ShrinkSequence();
                        }
                    }
                }
            }
            else
            {
                if (m_canDespawn)
                {
                    m_despawnTimer += Time.deltaTime;
                    if (m_despawnTimer >= m_lifeTime)
                    {
                        m_isDespawning = true;
                        ShrinkSequence();
                    }
                }
                else
                {
                    m_despawnTimer = 0.0f;
                }
            }
        }
    }

    private void ShrinkSequence()
    {
        gameObject.layer = 0;
        
        transform.DOScale(Vector3.zero, m_shrinkTime).SetEase(Ease.InSine).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.relativeVelocity.magnitude > m_velocityThreshold)
        {
            if (other.collider.CompareTag("Enemy"))
            {
                var health = other.gameObject.GetComponent<HealthComponent>();
                if (health != null)
                {
                    health.DamageEvent.Invoke(m_damage, gameObject);
                }
            }
        }
        
    }
}
