using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnComponent : MonoBehaviour
{
    private Collider2D m_col;
    private int m_intersectCnt = 0;
    
    private void Start()
    {
        m_col = GetComponent<Collider2D>();
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return new WaitForSeconds(0.2f);
        if (m_intersectCnt <= 0)
        {
            m_col.isTrigger = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Rope") && !other.CompareTag("Enemy") && !other.CompareTag("Linkable"))
        {
            m_intersectCnt += 1;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Rope") && !other.CompareTag("Enemy") && !other.CompareTag("Linkable"))
        {
            m_intersectCnt -= 1;

            if (m_intersectCnt <= 0)
            {
                SingletonMaster.Instance.FeelManager.m_wallParticles.PlayFeedbacks(transform.position);
                m_col.isTrigger = false;
            }
        }
    }
}
