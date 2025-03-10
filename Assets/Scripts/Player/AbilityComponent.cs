using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// TODO: Refactor this into a loot class
public class AbilityComponent : MonoBehaviour
{
    [Header("Ability Settings")]
    public AbilityScriptable m_ability;
    public int m_maxUse = 5;
    public bool m_canDespawn = true;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private AbilityManager.AbilityTypes m_type;
    [SerializeField] private Color m_coolDownColor;
    [SerializeField] private RopeComponent m_ropeComponent;
    [SerializeField] private GameObject m_text;
    [SerializeField] private bool m_showText = false;
    [SerializeField] private float m_lifeTime = 10.0f;
    
    [Header("Damage Settings")]
    [SerializeField] private float m_damage = 1.5f;
    [SerializeField] private float m_velocityThreshold = 10.0f;
    
    [Header("Visual Settings")]
    [SerializeField] private float m_shrinkTime = 0.15f;

    private bool m_isDespawning = false;
    private bool m_isConnected = false;
    private bool m_canActivate = true;
    private Color m_orgColor;
    private int m_use = 0;
    private float m_despawnTimer = 0.0f;
    
    private void Start()
    {
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
        
        SingletonMaster.Instance.AbilityManager.ActivateAbility.AddListener(OnAbilityActivated);
        SingletonMaster.Instance.AbilityManager.AbilityFinished.AddListener(OnAbilityFinished);

        m_orgColor = m_spriteRenderer.color;

        if (!m_showText)
        {
            m_text.SetActive(false);
        }
    }

    private void OnAbilityFinished(AbilityManager.AbilityTypes type)
    {
        if (m_isConnected && m_type == type)
        {
            m_use++;
            
            if (m_use == m_maxUse)
            {
                m_ropeComponent.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
                m_isDespawning = true;
                ShrinkSequence();
            }
        }
    }

    private void OnAbilityActivated(AbilityManager.AbilityTypes type)
    {
        if (m_isConnected && m_type == type && m_canActivate && m_use < m_maxUse)
        {
            m_canActivate = false;
            Color newColor = m_coolDownColor;
            newColor.a = 0.5f;
            m_spriteRenderer.color = newColor;
            StartCoroutine(StartColorFade());
        }
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
    }
    
    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_isConnected = false;
            m_ability.RemoveLink();

            if (m_showText)
            {
                m_text.SetActive(true);
            }
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_isConnected = true;
            m_ability.AddLink();

            m_despawnTimer = 0.0f;
            
            if (m_showText)
            {
                m_text.SetActive(false);
            }
        }
    }

    private IEnumerator StartColorFade()
    {
        float time = 0.0f;
        float rate = (1.0f - 0.5f) / m_ability.m_coolDown;
        
        while (time <= m_ability.m_coolDown)
        {
            time += Time.deltaTime;
            Color newColor = m_spriteRenderer.color;

            if (newColor.a < 1.0f)
            {
                newColor.a += rate * Time.deltaTime;
            }
            else
            {
                newColor.a = 1.0f;
            }
            
            // Debug.Log(newColor.a);
            
            m_spriteRenderer.color = newColor;
            yield return null;
        }

        m_spriteRenderer.color = m_orgColor;
        m_canActivate = true;
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

    private void Update()
    {
        if (!m_isDespawning && SingletonMaster.Instance.PlayerBase != null)
        {
            if (m_canDespawn)
            {
                if (!m_isConnected)
                {
                    m_despawnTimer += Time.deltaTime;
                    if (m_despawnTimer >= m_lifeTime)
                    {
                        m_isDespawning = true;
                        ShrinkSequence();
                    }
                }
            }
            else
            {
                m_despawnTimer = 0.0f;
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
}
