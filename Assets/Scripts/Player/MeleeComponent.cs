using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeComponent : MonoBehaviour
{
    [Header("Damage Settings")] 
    [SerializeField] private int m_durability = 20;
    [SerializeField] private float m_damage = 10.0f;
    [SerializeField] private float m_velocityThreshold = 2.0f;
    [SerializeField] private float m_knockBackStrength = 100.0f;
    
    [Header("Visual Settings")] 
    [SerializeField] private TrailRenderer m_trail;

    private Rigidbody2D m_RB;
    private bool m_canDamage = false;
    private bool m_canFlick = false;
    private bool m_isMouseDown = false;
    
    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
        
        SingletonMaster.Instance.EventManager.StartFireEvent.AddListener(StartFiring);
        SingletonMaster.Instance.EventManager.StopFireEvent.AddListener(StopFiring);
        
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.StartFireEvent.RemoveListener(StartFiring);
        SingletonMaster.Instance.EventManager.StopFireEvent.RemoveListener(StopFiring);
        
        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
    }

    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_canFlick = false;
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_canFlick = true;
        }
    }

    private void StopFiring()
    {
        m_isMouseDown = false;
    }

    private void StartFiring()
    {
        m_isMouseDown = true;
    }

    private void FixedUpdate()
    {
        if (m_RB.velocity.magnitude > m_velocityThreshold)
        {
            m_trail.enabled = true;
            m_canDamage = true;
        }
        else
        {
            m_trail.enabled = false;
            m_canDamage = false;
        }
    }

    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);

        if (m_isMouseDown)
        {
            
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy") && m_canDamage)
        {
            BaseEnemyBehavior enemy = other.gameObject.GetComponent<BaseEnemyBehavior>();
            if (enemy != null)
            {
                Rigidbody2D enemyRB = enemy.gameObject.GetComponent<Rigidbody2D>();
                if (enemyRB != null)
                {
                    enemyRB.AddForce(m_RB.velocity.normalized * m_knockBackStrength, ForceMode2D.Impulse);
                }

                m_durability--;
                enemy.EnemyDamagedEvent.Invoke(m_damage);
            }
        }
    }
}
