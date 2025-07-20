using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO: Refactor this into a loot class
public class AbilityComponent : MonoBehaviour
{
    [Header("Ability Settings")]
    public AbilityScriptable m_ability;
    public int m_maxUse = 5;
    public bool m_canDespawn = true;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    public AbilityManager.AbilityTypes m_type;
    [SerializeField] private Color m_coolDownColor;
    [SerializeField] private RopeComponent m_ropeComponent;
    [SerializeField] private float m_lifeTime = 10.0f;
    
    [Header("Damage Settings")]
    [SerializeField] private float m_damage = 1.5f;
    [SerializeField] private float m_velocityThreshold = 10.0f;
    
    [Header("Visual Settings")]
    [SerializeField] private float m_shrinkTime = 0.15f;
    [SerializeField] private AnimationCurve m_curve;
    [SerializeField] private float m_scaleFactor = 1.25f;
    [SerializeField] private List<Color> m_collectedColor = new List<Color>();
    [SerializeField] private List<SpriteRenderer> m_spriteRenderers = new List<SpriteRenderer>();
    
    [Header("Control Prompts Settings")]
    [SerializeField] private InputActionReference m_connectAction;
    [SerializeField] private InputActionReference m_disconnectAction;
    [SerializeField] private GameObject m_connectPrompt;
    [SerializeField] private TMP_Text m_connectText;
    [SerializeField] private GameObject m_disconnectPrompt;
    [SerializeField] private TMP_Text m_disconnectText;
    [SerializeField] private bool m_showPrompt = false;

    private List<Color> m_uncollectedColor = new List<Color>();
    private bool m_isDespawning = false;
    private bool m_isConnected = false;
    [SerializeField] private bool m_canActivate = true;
    [SerializeField] private int m_use = 0;
    private float m_despawnTimer = 0.0f;
    private Vector3 m_orgScale;
    
    [SerializeField] private GameObject m_leftClickIcon;
    [SerializeField] private GameObject m_rightClickIcon;

    private bool m_isFirstTime = true;
    private void Start()
    {
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
        
        SingletonMaster.Instance.AbilityManager.ActivateAbility.AddListener(OnAbilityActivated);
        SingletonMaster.Instance.AbilityManager.AbilityFinished.AddListener(OnAbilityFinished);

        float scale = m_curve.Evaluate((float)(m_maxUse - m_use) / m_maxUse) * m_scaleFactor;
        transform.localScale = Vector3.one * scale;
        m_orgScale = transform.localScale;

        if (!m_showPrompt)
        {
            m_connectPrompt.SetActive(false);
            m_disconnectPrompt.SetActive(false);
        }
        
        foreach (var sp in m_spriteRenderers)
        {
            m_uncollectedColor.Add(sp.color);
        }
        
        // Record spawn
        MetricsManager.Instance.m_metricsData.RecordAblitySpawn(m_ability.m_name);
        
        string connectText = InputControlPath.ToHumanReadableString(m_connectAction.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        string disconnectText = InputControlPath.ToHumanReadableString(m_disconnectAction.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

        if (connectText == "Left Button")
        {
            m_connectText.enabled = false;
            m_leftClickIcon.SetActive(true);
        }

        if (disconnectText == "Right Button")
        {
            m_disconnectText.enabled = false;
            m_rightClickIcon.SetActive(true);
        }
        
        m_connectText.text = connectText;
        m_disconnectText.text = disconnectText;
        
    }
    
    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
        
        SingletonMaster.Instance.AbilityManager.ActivateAbility.RemoveListener(OnAbilityActivated);
        SingletonMaster.Instance.AbilityManager.AbilityFinished.RemoveListener(OnAbilityFinished);
    }

    public void ForceDropAbility()
    {
        m_ropeComponent.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
        m_isDespawning = true;
        ShrinkSequence();
    }

    private void OnAbilityFinished(AbilityManager.AbilityTypes type)
    {
        if (m_isConnected && m_type == type)
        {
            if (m_use >= m_maxUse)
            {
                m_ropeComponent.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
                m_isDespawning = true;
                ShrinkSequence();
            }
        }
    }

    private void OnAbilityActivated(AbilityManager.AbilityTypes type)
    {
        if (m_isConnected && m_type == type && m_canActivate && m_use < m_maxUse && 
            (SingletonMaster.Instance.PlayerBase.m_dashPool.Count > 0 && SingletonMaster.Instance.PlayerBase.m_dashPool[^1] == this || 
             SingletonMaster.Instance.PlayerBase.m_knockBackPool.Count > 0 && SingletonMaster.Instance.PlayerBase.m_knockBackPool[^1] == this))
        {
            m_use++;
            float scale = m_curve.Evaluate((float)(m_maxUse - m_use) / m_maxUse) * m_scaleFactor;
            transform.localScale = Vector3.one * scale;
            
            Debug.Log("Ability " + type + " use: " + m_use);
            
            m_canActivate = false;

            for (int i = 0; i < m_spriteRenderers.Count; i++)
            {
                Color newColor = m_coolDownColor;
                newColor.a = 0.5f;
                m_spriteRenderers[i].color = newColor;
                m_spriteRenderers[i].DOFade(1.0f, m_ability.m_coolDown).SetEase(Ease.Linear).OnComplete(() =>
                {
                    if (m_isConnected)
                    {
                        for (int i = 0; i < m_spriteRenderers.Count; i++)
                        {
                            m_spriteRenderers[i].color = m_collectedColor[i];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < m_spriteRenderers.Count; i++)
                        {
                            m_spriteRenderers[i].color = m_uncollectedColor[i];
                        }
                    }

                    m_canActivate = true;
                });
            }

            // For tutorial
            if (GameManager.Instance.m_levelData.m_needsTutorial)
            {
                SingletonMaster.Instance.EventManager.TutorialPlayerAbility.Invoke();
            }

            if (m_isFirstTime)
            {
                m_isFirstTime = false;
                // Record Activation
                MetricsManager.Instance.m_metricsData.RecordAblityActivate(m_ability.m_name);
            }
        }
    }
    
    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_isConnected = false;
            m_ability.RemoveLink();

            SingletonMaster.Instance.PlayerBase.RemoveAbilityFromStack(this);

            if (m_canActivate)
            {
                for (int i = 0; i < m_spriteRenderers.Count; i++)
                {
                    m_spriteRenderers[i].color = m_uncollectedColor[i];
                }
            }
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_isConnected = true;
            m_ability.AddLink();

            if (m_canActivate)
            {
                for (int i = 0; i < m_spriteRenderers.Count; i++)
                {
                    m_spriteRenderers[i].color = m_collectedColor[i];
                }
            }

            m_despawnTimer = 0.0f;
            
            if (m_showPrompt)
            {
                m_connectPrompt.SetActive(false);
                m_showPrompt = false;
                SingletonMaster.Instance.EventManager.TutorialPlayerLinkedAbility.Invoke();
            }
            
            // Add to player's ability stack
            SingletonMaster.Instance.PlayerBase.AddAbilityToStack(this);
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
            SingletonMaster.Instance.PlayerBase.RemoveAbilityFromStack(this);
            Destroy(gameObject);
        });
    }
}
