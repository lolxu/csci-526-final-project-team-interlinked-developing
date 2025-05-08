using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class MovingEnvironmentals : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private GameObject m_end;
    [SerializeField] private float m_moveDuration = 5.0f;
    [SerializeField] private Ease m_ease;
    
    [Header("Path Settings")]
    [SerializeField] private bool m_needsPath = false;
    [SerializeField] private List<GameObject> m_waypoints = new List<GameObject>();

    private Vector3 m_start;
    private Vector3[] m_path;

    private void Start()
    {
        m_start = transform.position;

        // Add to path array if we have a path
        if (m_needsPath)
        {
            m_path = new Vector3[m_waypoints.Count];
            for (int i = 0; i < m_waypoints.Count; i++)
            {
                m_path[i] = m_waypoints[i].transform.position;
            }
        }
    }

    public void StartMoving()
    {
        gameObject.SetActive(true);
        transform.DOMove(m_end.transform.position, m_moveDuration)
            .SetEase(m_ease);

        if (m_needsPath)
        {
            transform.DOPath(m_path, m_moveDuration, PathType.CatmullRom)
                .SetEase(m_ease)
                .OnComplete(() =>
                {
                    // Disable object if it's a moving path one
                    gameObject.SetActive(false);
                    Debug.Log("Path animation completed!");
                });
        }
    }

    public void MoveBack()
    {
        transform.DOMove(m_start, m_moveDuration)
            .SetEase(m_ease);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_end.transform.position, 1.0f);
        Gizmos.DrawLine(transform.position, m_end.transform.position);
    }
}
