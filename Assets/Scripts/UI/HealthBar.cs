using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image m_healthFill;
    public Image m_background;
    public HealthComponent m_healthComp;
    public bool m_isWorldSpace = false;
    public float m_showTime = 1.0f;
    public Vector3 m_offset;
    private float m_timer = 1.0f;
    private bool m_hasRegistered = false;

    private void Start()
    {
        if (m_isWorldSpace)
        {
            m_healthComp.DamageEvent.AddListener(OnDamage);
        }
    }
    private void OnDamage(float amount, GameObject instigator)
    {
        m_timer = 0.0f;
        m_healthFill.enabled = true;
        m_background.enabled = true;
    }

    private void Update()
    {
        if (m_isWorldSpace)
        {
            transform.position = Camera.main.WorldToScreenPoint(m_healthComp.transform.position + m_offset);
            m_healthFill.fillAmount = m_healthComp.m_health / m_healthComp.m_maxHealth;

            if (!m_healthComp.m_isHealing)
            {
                m_timer += Time.deltaTime;
                if (m_timer >= m_showTime)
                {
                    m_timer = m_showTime;
                    m_healthFill.enabled = false;
                    m_background.enabled = false;
                }
            }
            else
            {
                m_healthFill.enabled = true;
                m_background.enabled = true;
            }
        }
        else
        {
            m_healthFill.fillAmount = m_healthComp.m_health / m_healthComp.m_maxHealth;
        }
    }
}
