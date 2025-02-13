using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    [Header("Spawning Settings")] 
    public float m_waveTime = 60.0f;
    public float m_waveCoolDown = 30.0f;
    public int m_maxEnemyCount = 20;
    public float m_spawnPadding = 1.0f;
    public bool m_canSpawn = false;
    
    private List<GameObject> m_enemies = new List<GameObject>();
    private float m_timer = 0.0f;

    private void Start()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(RemoveEnemy);
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(SpawnLoot);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(RemoveEnemy);
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(SpawnLoot);
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name != SingletonMaster.Instance.HubName)
        {
            StartCoroutine(StartSpawnTimeout());
        }
        else
        {
            m_enemies.Clear();
            m_canSpawn = false;
        }
    }

    private IEnumerator StartSpawnTimeout()
    {
        yield return new WaitForSeconds(1.25f);
        m_canSpawn = true;
        m_timer = m_waveTime;
    }

    private IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(m_waveCoolDown);
        if (SceneManager.GetActiveScene().name != SingletonMaster.Instance.HubName)
        {
            m_canSpawn = true;
        }
    }

    void Update()
    {
        if (m_canSpawn)
        {
            int enemyCount = m_enemies.Count;
            if (enemyCount < m_maxEnemyCount)
            {
                SpawnEnemies(m_maxEnemyCount - enemyCount);
            }

            if (m_timer <= 0.0f)
            {
                m_waveTime += Random.Range(10.0f, 30.0f);
                m_timer = m_waveTime;
                m_maxEnemyCount += Random.Range(1, 5);
                m_canSpawn = false;
                SingletonMaster.Instance.EventManager.CooldownStarted.Invoke(m_waveCoolDown);
                StartCoroutine(StartCooldown());
                Debug.Log("Starting wave cool down. You have " + m_waveCoolDown + " seconds.");
            }
            else
            {
                m_timer -= Time.deltaTime;
            }
        }
    }

    private void SpawnEnemies(int num)
    {
        for (int i = 0; i < num; i++)
        {
            Vector2 spawnPos = Camera.main.transform.position;
            spawnPos += GetRandomSpawnPosition();
            EnemyScriptable.Enemy newEnemy = SingletonMaster.Instance.EnemySpawnScriptableObject.GetRandomEnemyToSpawn();
            GameObject enemyPrefab = newEnemy.m_prefab;
            GameObject spawned = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            spawned.GetComponent<BaseEnemyBehavior>().m_lootDropRate = newEnemy.m_lootSpawnRate;
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
        if (roll <= enemy.GetComponent<BaseEnemyBehavior>().m_lootDropRate)
        {
            GameObject loot = SingletonMaster.Instance.LootSpawnScriptableObject.GetRandomLootToSpawn().m_prefab;
            Instantiate(loot, enemy.transform.position, Quaternion.identity);
        }
    }

    private void RemoveEnemy(GameObject enemy)
    {
        m_enemies.Remove(enemy);
        
        // Checking for any connected stuff to this enemy
        RopeComponent rc = enemy.GetComponent<RopeComponent>();
        for (int i = rc.m_connectedTo.Count - 1; i >= 0; --i)
        {
            var connectedObj = rc.m_connectedTo[i];
            connectedObj.transform.SetParent(null, true);
            connectedObj.GetComponent<RopeComponent>().DetachEnemy(enemy);
        }
        
        Destroy(enemy);
    }
}
