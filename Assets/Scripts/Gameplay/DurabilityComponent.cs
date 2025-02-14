using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DurabilityComponent : MonoBehaviour
{
    [Header("Durability Settings")]
    public int m_maxDurability = 10;
    public int m_currentDurability = 10;
    public UnityEvent UsedDurabilityEvent = new UnityEvent();
    
    [Header("Visual Settings")]
    [SerializeField] private float m_shrinkSpeed = 1.25f;

    private bool m_isDespawning = false;
    private GameObject m_durabilityBar = null;

    private void Start()
    {
        // Create durability
        m_durabilityBar = SingletonMaster.Instance.UI.AddDurabilityBar(this);
    }

    private void OnDisable()
    {
        if (m_durabilityBar != null)
        {
            Destroy(m_durabilityBar);
        }
    }

    public void UseDurability()
    {
        m_currentDurability--;
        UsedDurabilityEvent.Invoke();
    }

    private void Update()
    {
        if (m_currentDurability == 0)
        {
            RopeComponent rc = GetComponent<RopeComponent>();
            if (rc != null && !m_isDespawning)
            {
                m_isDespawning = true;
                rc.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
                StartCoroutine(DestroySequence());
            }
        }
    }

    private IEnumerator DestroySequence()
    {
        while (transform.localScale.x >= 0.0f)
        {
            transform.localScale -= Vector3.one * m_shrinkSpeed * Time.fixedDeltaTime;
            yield return null;
        }
        transform.localScale = Vector3.zero;
        yield return null;
        Destroy(gameObject);
    }
}
