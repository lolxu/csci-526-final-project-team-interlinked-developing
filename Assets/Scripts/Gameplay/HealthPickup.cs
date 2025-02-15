using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class HealthPickup : MonoBehaviour
{
    
    [Header("Health pickup settings")]
    public float m_healValue = 5.0f;
    public float m_maxValue = 10.0f;
    [SerializeField] private float m_healRate = 1.0f;
    [SerializeField] private RopeComponent m_ropeComponent;
    [SerializeField] private float m_lifeTime = 20.0f;
    
    [Header("Visual Settings")]
    [SerializeField] private float m_scaleFactor = 1.5f;
    [SerializeField] private AnimationCurve m_curve;
    [SerializeField] private Color m_uncollectedColor;
    [SerializeField] private Color m_collectedColor;
    [SerializeField] private float m_shrinkSpeed = 1.25f;
    
    [Header("Damage Settings")]
    [SerializeField] private float m_damage = 1.5f;
    [SerializeField] private float m_velocityThreshold = 10.0f;
    
    private Coroutine shrinkCoroutine = null;
    private float m_despawnTimer = 0.0f;
    private SpriteRenderer m_spriteRend;
    private bool m_isHealing = false;

    private void Start()
    {
        m_spriteRend = GetComponent<SpriteRenderer>();
        m_healValue = Random.Range(1, m_maxValue);
        float scale = m_curve.Evaluate((float)m_healValue / m_maxValue) * m_scaleFactor;
        transform.localScale = Vector3.one * scale;

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
            m_spriteRend.color = m_uncollectedColor;
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && instigator.CompareTag("Player"))
        {
            m_isHealing = true;
            m_spriteRend.color = m_collectedColor;
        }
    }

    private void Update()
    {
        if (shrinkCoroutine == null && SingletonMaster.Instance.PlayerBase != null)
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
                            shrinkCoroutine = StartCoroutine(ShrinkSequence());
                        }
                    }
                }
            }
            else
            {
                m_despawnTimer += Time.deltaTime;
                if (m_despawnTimer >= m_lifeTime)
                {
                    shrinkCoroutine = StartCoroutine(ShrinkSequence());
                }
            }
        }
    }

    public void StartShrinking()
    {
        if (shrinkCoroutine == null)
        {
            shrinkCoroutine = StartCoroutine(ShrinkSequence());
        }
    }

    private IEnumerator ShrinkSequence()
    {
        gameObject.layer = 0;
        while (transform.localScale.x >= 0.0f)
        {
            transform.localScale -= Vector3.one * m_shrinkSpeed * Time.fixedDeltaTime;
            yield return null;
        }
        transform.localScale = Vector3.zero;
        yield return null;
        Destroy(gameObject);
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
