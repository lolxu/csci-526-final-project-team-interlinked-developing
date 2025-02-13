using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DurabilityBar : MonoBehaviour
{
    public Image m_durabilityFill;
    public DurabilityComponent m_durabilityComp;
    public float m_showTime = 1.0f;
    public Vector3 m_offset;
    private float m_timer = 1.0f;
    private bool m_hasRegistered = false;

    private void Start()
    {
        m_durabilityComp.UsedDurabilityEvent.AddListener(OnUseDurability);
    }
    private void OnUseDurability()
    {
        m_timer = 0.0f;
        m_durabilityFill.transform.parent.gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(m_durabilityComp.transform.position + m_offset);
        m_durabilityFill.fillAmount = (float)m_durabilityComp.m_currentDurability / m_durabilityComp.m_maxDurability;

        m_timer += Time.deltaTime;
        if (m_timer >= m_showTime)
        {
            m_timer = m_showTime;
            m_durabilityFill.transform.parent.gameObject.SetActive(false);
        }
    }
}
