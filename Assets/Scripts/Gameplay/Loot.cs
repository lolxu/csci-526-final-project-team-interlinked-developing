using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loot : MonoBehaviour
{
    [SerializeField] private Color m_uncollectedColor;
    [SerializeField] private Color m_collectedColor;
    [SerializeField] private float m_shrinkSpeed = 1.25f;
    [SerializeField] private float m_lifeTime = 120.0f;
    private Coroutine shrinkCoroutine = null;
    private float m_timer = 0.0f;
    private SpriteRenderer m_spriteRend;

    private void Start()
    {
        m_spriteRend = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (gameObject.layer == SingletonMaster.Instance.PLAYER_LAYER && m_spriteRend.color != m_collectedColor)
        {
            m_spriteRend.color = m_collectedColor;
        }
        else if (gameObject.layer == SingletonMaster.Instance.UNCONNECTED_LAYER &&
                 m_spriteRend.color != m_uncollectedColor)
        {
            m_spriteRend.color = m_uncollectedColor;
        }

        if (gameObject.layer == SingletonMaster.Instance.UNCONNECTED_LAYER && SceneManager.GetActiveScene().name == SingletonMaster.Instance.BattlefieldName)
        {
            m_timer += Time.deltaTime;
            if (m_timer >= m_lifeTime && shrinkCoroutine == null)
            {
                shrinkCoroutine = StartCoroutine(ShrinkSequence());
            }
        }
        else
        {
            m_timer = 0.0f;
        }
    }

    public void StartShrinking()
    {
        if (shrinkCoroutine == null)
        {
            shrinkCoroutine = StartCoroutine(ShrinkSequence());
            SingletonMaster.Instance.EventManager.LootCollected.Invoke();
        }
    }

    private IEnumerator ShrinkSequence()
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
