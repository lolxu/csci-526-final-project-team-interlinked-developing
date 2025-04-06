using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MovingEnvironmentals : MonoBehaviour
{
    [SerializeField] private GameObject m_end;
    [SerializeField] private float m_moveDuration = 5.0f;
    [SerializeField] private Ease m_ease;

    private Vector3 m_start;

    private void Start()
    {
        m_start = transform.position;
    }

    public void StartMoving()
    {
        gameObject.SetActive(true);
        transform.DOMove(m_end.transform.position, m_moveDuration)
            .SetEase(m_ease);
    }

    public void MoveBack()
    {
        transform.DOMove(m_start, m_moveDuration)
            .SetEase(m_ease)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_end.transform.position, 1.0f);
        Gizmos.DrawLine(transform.position, m_end.transform.position);
    }
}
