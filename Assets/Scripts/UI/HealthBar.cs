using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image m_healthFill;
    public HealthComponent m_healthComp;
    public float m_showTime = 1.0f;
    public Vector3 m_offset;
    private float m_timer = 0.0f;
    private bool m_hasRegistered = false;

    private void Start()
    {
        m_healthComp.DamageEvent.AddListener(OnDamage);
    }
    private void OnDamage(float amount, GameObject instigator)
    {
        m_timer = 0.0f;
        m_healthFill.transform.parent.gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(m_healthComp.transform.position + m_offset);
        m_healthFill.fillAmount = m_healthComp.m_health / m_healthComp.m_maxHealth;

        m_timer += Time.deltaTime;
        if (m_timer >= m_showTime)
        {
            m_timer = m_showTime;
            m_healthFill.transform.parent.gameObject.SetActive(false);
        }
    }
}
