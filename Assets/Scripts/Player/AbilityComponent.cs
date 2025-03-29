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
    [SerializeField] private List<Color> m_collectedColor = new List<Color>();
    [SerializeField] private List<SpriteRenderer> m_spriteRenderers = new List<SpriteRenderer>();

    private List<Color> m_uncollectedColor = new List<Color>();
    private bool m_isDespawning = false;
    private bool m_isConnected = false;
    private bool m_canActivate = true;
    private int m_use = 0;
    private float m_despawnTimer = 0.0f;
    private Vector3 m_orgScale;
    
    private void Start()
    {
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
        
        SingletonMaster.Instance.AbilityManager.ActivateAbility.AddListener(OnAbilityActivated);
        SingletonMaster.Instance.AbilityManager.AbilityFinished.AddListener(OnAbilityFinished);

        m_orgScale = transform.localScale;
        
        // Start spawn as trigger
        GetComponent<Collider2D>().isTrigger = true;

        if (!m_showText)
        {
            m_text.SetActive(false);
        }
        
        foreach (var sp in m_spriteRenderers)
        {
            m_uncollectedColor.Add(sp.color);
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
            float ratio = (m_maxUse - m_use) / (float)m_maxUse;
            transform.localScale = m_orgScale * ratio;
            
            m_canActivate = false;
            Color orgColor = m_spriteRenderer.color;
            Color newColor = m_coolDownColor;
            newColor.a = 0.5f;
            m_spriteRenderer.color = newColor;

            m_spriteRenderer.DOFade(1.0f, m_ability.m_coolDown).SetEase(Ease.Linear).OnComplete(() =>
            {
                m_spriteRenderer.color = orgColor;
                m_canActivate = true;
            });
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
            
            for (int i = 0; i < m_spriteRenderers.Count; i++)
            {
                m_spriteRenderers[i].color = m_uncollectedColor[i];
            }

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
            
            for (int i = 0; i < m_spriteRenderers.Count; i++)
            {
                m_spriteRenderers[i].color = m_collectedColor[i];
            }

            m_despawnTimer = 0.0f;
            
            if (m_showText)
            {
                m_text.SetActive(false);
            }
        }
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
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Background") && GetComponent<Collider2D>().isTrigger)
        {
            GetComponent<Collider2D>().isTrigger = false;
        }
    }
}
