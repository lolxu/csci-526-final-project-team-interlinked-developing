using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    // Needs to be object pooled perhaps
    public float m_speed;
    public float m_lifeTime = 5.0f;
    public Vector2 m_direction;
    public int m_penetrateNum = 1;
    
    private Rigidbody2D m_RB;

    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        m_RB.velocity = m_direction * m_speed * Time.fixedDeltaTime;
        m_lifeTime -= Time.fixedDeltaTime;
        if (m_lifeTime < 0.0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Hit " + other.gameObject);
        m_penetrateNum--;
        //if (m_penetrateNum <= 0)
        {
            Destroy(gameObject);
        }
    }
}
