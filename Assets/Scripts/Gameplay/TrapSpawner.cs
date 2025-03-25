using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrapSpawner : MonoBehaviour
{
    public List<GameObject> m_traps;
    public float m_maxTimeout = 20.0f;

    private float m_timeout = 0.0f;
    private float m_timer = 0.0f;
    private BoxCollider2D m_collider;
    
    private void Start()
    {
        m_collider = GetComponent<BoxCollider2D>();
        m_timeout = Random.Range(0.0f, m_maxTimeout);
    }

    private void Update()
    {
        if (m_timer >= m_timeout)
        {
            m_timer = 0.0f;
            m_timeout = Random.Range(0.0f, m_maxTimeout);
            
            SpawnTrapRandomly();
        }

        m_timer += Time.deltaTime;
    }

    private void SpawnTrapRandomly()
    {
        float randX = Random.Range(-m_collider.size.x / 2.0f + transform.position.x, m_collider.size.x / 2.0f + transform.position.x);
        Vector3 spawnPos = new Vector3(randX, transform.position.y, 0.0f);

        Instantiate(m_traps[Random.Range(0, m_traps.Count)], spawnPos, Quaternion.identity);
    }
}
