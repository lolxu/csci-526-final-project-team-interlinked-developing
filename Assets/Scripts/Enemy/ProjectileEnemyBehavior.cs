using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnemyBehavior : BaseEnemyBehavior
{
    [Header("Projectile Enemy Settings")] 
    [SerializeField] private GameObject m_projectile;
    [SerializeField] private float m_fireDistance = 5.0f;
    [SerializeField] private float m_fireCooldown = 1.25f;
    [SerializeField] private float m_projectileSpawnOffset = 1.0f;
    [SerializeField] private float m_summonTime = 0.75f;

    private GameObject m_lastProjectile = null;
    private Coroutine m_spawnCoroutine = null;
    private bool m_spawned = true;
    private float m_shootTimer = 0.0f;
    
    protected override void OnStart()
    {
        base.OnStart();

        m_shootTimer = m_fireCooldown;
    }

    protected override void OnBeingDisabled()
    {
        base.OnBeingDisabled();

        if (m_spawnCoroutine != null)
        {
            StopCoroutine(m_spawnCoroutine);
        }
        if (m_lastProjectile != null)
        {
            Destroy(m_lastProjectile);
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (m_spawned)
        {
            m_shootTimer += Time.deltaTime;
            if (m_shootTimer > m_fireCooldown)
            {
                m_shootTimer = m_fireCooldown;
            }
        }

        PlayerBase pb = SingletonMaster.Instance.PlayerBase;
        if (pb != null)
        {
            Vector3 playerPos = pb.gameObject.transform.position;
            Vector3 myPos = transform.position;

            float dist = Vector3.Distance(myPos, playerPos);

            if (dist < m_fireDistance && m_shootTimer >= m_fireCooldown)
            {
                // Fire projectile
                // TODO: Add a spawn in animation
                m_spawnCoroutine = StartCoroutine(SpawnProjectile());
            }
        }
    }

    private IEnumerator SpawnProjectile()
    {
        m_spawned = false;      
        m_shootTimer = 0.0f;
        
        Vector3 myPos = transform.position;
        Vector3 playerPos = SingletonMaster.Instance.PlayerBase.gameObject.transform.position;
        Vector3 moveDir = (playerPos - myPos).normalized;
        Vector3 spawnPos = myPos + moveDir * m_projectileSpawnOffset;
        GameObject projectile = Instantiate(m_projectile, spawnPos, Quaternion.identity);
        m_lastProjectile = projectile;
        projectile.transform.localScale = Vector3.zero;
        
        float timer = 0.0f;
        float rate = 1.0f / m_summonTime;
        
        while (timer < m_summonTime)
        {
            timer += Time.deltaTime;
            projectile.transform.localScale += Vector3.one * rate * Time.deltaTime;

            if (SingletonMaster.Instance.PlayerBase != null)
            {
                myPos = transform.position;
                playerPos = SingletonMaster.Instance.PlayerBase.gameObject.transform.position;
                moveDir = (playerPos - myPos).normalized;
                spawnPos = myPos + moveDir * m_projectileSpawnOffset;
                projectile.transform.position = spawnPos;
            }
            
            yield return null;
        }
        
        projectile.GetComponent<EnemyProjectile>().Spawned();
        m_spawned = true;
    }
}
