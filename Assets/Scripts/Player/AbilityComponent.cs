using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : MonoBehaviour
{
    public AbilityScriptable m_ability;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private AbilityManager.AbilityTypes m_type;
    [SerializeField] private Color m_coolDownColor;

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
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_ability.m_enabled = true;
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
            
            Debug.Log(newColor.a);
            
            m_spriteRenderer.color = newColor;
            yield return null;
        }

        m_spriteRenderer.color = m_orgColor;
        m_canActivate = true;
    }
}
