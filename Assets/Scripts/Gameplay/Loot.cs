using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Loot : MonoBehaviour
{
    public int m_value { private set; get; } = 0;
    public int m_maxValue = 10;
    public float m_scaleFactor = 0.5f;
    public TextMeshPro m_valueText;
    [SerializeField] private AnimationCurve m_curve;
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
        m_value = Random.Range(1, m_maxValue);
        float scale = transform.localScale.x + m_curve.Evaluate((float)m_value / (float)m_maxValue);
        m_valueText.text = m_value.ToString();
        if (m_value > 1)
        {
            transform.localScale *= scale * m_scaleFactor;
        }
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
            SingletonMaster.Instance.EventManager.LootCollected.Invoke(m_value);
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
