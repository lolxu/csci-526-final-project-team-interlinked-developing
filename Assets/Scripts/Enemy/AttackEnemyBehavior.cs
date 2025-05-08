using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyBehavior : BaseEnemyBehavior
{
    [Header("Chase Settings")]
    public float moveSpeed = 3.0f;
    public float stoppingDistance = 0.5f;
    
    private Transform playerTransform;

    protected override void Start()
    {
        base.Start();
        
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            playerTransform = SingletonMaster.Instance.PlayerBase.transform;
        }
    }

    protected override void Update()
    {
        base.Update();
        
        if (playerTransform == null) return;

        // Move towards the player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > stoppingDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerBase player = collision.gameObject.GetComponent<PlayerBase>();
            if (player != null)
            {
                player.m_healthComponent.DamageEvent.Invoke(m_damage, gameObject);
                Destroy(gameObject); // Enemy crashes into the player and disappears
            }
        }
    }
}
