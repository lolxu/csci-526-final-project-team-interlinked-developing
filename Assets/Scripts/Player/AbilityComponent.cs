using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : MonoBehaviour
{
    [Header("Ability Settings")]
    public AbilityScriptable m_ability;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private AbilityManager.AbilityTypes m_type;
    [SerializeField] private Color m_coolDownColor;
    [SerializeField] private GameObject m_text;
    
    [Header("Damage Settings")]
    [SerializeField] private float m_damage = 1.5f;
    [SerializeField] private float m_velocityThreshold = 10.0f;

    private bool m_canActivate = true;
    private Color m_orgColor;
    
    private void Start()
    {
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
        
        SingletonMaster.Instance.AbilityManager.ActivateAbility.AddListener(AbilityActivated);

        m_orgColor = m_spriteRenderer.color;
    }

    private void AbilityActivated(AbilityManager.AbilityTypes type)
    {
        if (m_type == type && m_canActivate && m_ability.m_enabled)
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
            m_ability.m_enabled = false;
            m_text.SetActive(true);
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_ability.m_enabled = true;
            m_text.SetActive(false);
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
}
