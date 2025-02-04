using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    [Header("Spawning Settings")] 
    public float m_spawnTimeout = 10.0f;
    public float m_waveIncreaseTime = 60.0f;
    public int m_maxEnemyCount = 20;
    public float m_spawnPadding = 1.0f;

    [Header("Enemy Settings")] 
    [Range(0.0f, 1.0f)] public float m_enemyDropLootRate = 0.0f;
    public GameObject m_dropLoot;
    
    private List<GameObject> m_enemies = new List<GameObject>();
    private float m_timer;
    private float m_waveTimer;

    private void Start()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(RemoveEnemy);
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(SpawnLoot);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(RemoveEnemy);
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(SpawnLoot);
    }

    void Update()
    {
        int enemyCount = m_enemies.Count;
        if (enemyCount == 0 || m_timer <= 0.0f)
        {
            m_timer = m_spawnTimeout;
            SpawnEnemies(m_maxEnemyCount - enemyCount);
        
            // TODO: Remove magic numbers
            if (m_waveTimer <= 0.0f)
            {
                m_waveIncreaseTime += Random.Range(10.0f, 30.0f);
                m_waveTimer = m_waveIncreaseTime;
                m_maxEnemyCount += Random.Range(1, 5);
            }
        }

        m_waveTimer -= Time.deltaTime;
        m_timer -= Time.deltaTime;
    }

    private void SpawnEnemies(int num)
    {
        for (int i = 0; i < num; i++)
        {
            Vector2 spawnPos = Camera.main.transform.position;
            spawnPos += GetRandomSpawnPosition();
            EnemySpawnScriptable.Enemy newEnemy = SingletonMaster.Instance.EnemySpawnScriptableObject.GetRandomEnemyToSpawn();
            GameObject enemyPrefab = newEnemy.m_prefab;
            GameObject spawned = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            m_enemies.Add(spawned);
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;
        
        int side = Random.Range(0, 4); // 0:Top, 1:Bottom, 2:Left, 3:Right
        float x = 0f, y = 0f;

        switch (side) {
            case 0: // Top edge
                x = Random.Range(-camWidth, camWidth);
                y = camHeight + m_spawnPadding;
                break;
            case 1: // Bottom edge
                x = Random.Range(-camWidth, camWidth);
                y = -camHeight - m_spawnPadding;
                break;
            case 2: // Left edge
                x = -camWidth - m_spawnPadding;
                y = Random.Range(-camHeight, camHeight);
                break;
            case 3: // Right edge
                x = camWidth + m_spawnPadding;
                y = Random.Range(-camHeight, camHeight);
                break;
        }

        return new Vector2(x, y);
    }

    private void SpawnLoot(GameObject enemy)
    {
        float roll = Random.Range(0.0f, 1.0f);
        if (roll <= m_enemyDropLootRate)
        {
            Instantiate(m_dropLoot, enemy.transform.position, Quaternion.identity);
        }
    }

    private void RemoveEnemy(GameObject enemy)
    {
        m_enemies.Remove(enemy);
        Destroy(enemy);
    }
}
