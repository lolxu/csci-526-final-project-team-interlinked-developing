using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    [Header("Spawning Settings")] 
    public int m_waveCount = 0;
    public List<EnemySpawnScriptable> m_waves = new List<EnemySpawnScriptable>();
    public float m_waveCoolDown = 10.0f;
    public float m_spawnPadding = 1.0f;
    public LayerMask m_maskCheck;
    public bool m_forceNotSpawn = false;
    private bool m_canSpawn = false;

    [Serializable]
    public class WaveDangers
    {
        public List<GameObject> m_obstacles;
    }

    [Serializable]
    public class WaveEnvironmentalObstacles
    {
        public List<MovingEnvironmentals> m_envObstacles;
    }
    
    
    [Header("Obstacle Settings")] 
    [Tooltip("Make sure you have the same number of entries as waves")]
    public List<WaveDangers> m_waveDangers = new List<WaveDangers>();
    public List<WaveEnvironmentalObstacles> m_waveEnvironmentals = new List<WaveEnvironmentalObstacles>();
    public List<GameObject> m_allWaveDangers = new List<GameObject>();

    private EnemySpawnScriptable m_currentWave;
    private float m_waveTime = 0.0f;
    private int m_maxEnemyCount = 20;
    
    [Header("Enemy Tracking")]
    public List<GameObject> m_enemies = new List<GameObject>();

    private void Start()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(RemoveEnemy);
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(SpawnLoot);
        SingletonMaster.Instance.EventManager.LevelClearEvent.AddListener(OnLevelClear);
        
        foreach (var obstacle in m_allWaveDangers)
        {
            obstacle.SetActive(false);
        }
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(RemoveEnemy);
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(SpawnLoot);
        SingletonMaster.Instance.EventManager.LevelClearEvent.RemoveListener(OnLevelClear);
    }
    
    private void OnLevelClear()
    {
        foreach (var obstacle in m_allWaveDangers)
        {
            obstacle.SetActive(false);
        }
    }

    public void StartWaves()
    {
        StartCoroutine(StartMyWaves());
    }

    private IEnumerator StartMyWaves()
    {
        if (m_waves.Count > 0 && !m_forceNotSpawn)
        {
            m_canSpawn = true;
            m_currentWave = m_waves[0];
            m_maxEnemyCount = m_currentWave.m_maxEnemyCount;
            m_waveTime = m_currentWave.m_waveTime;

            yield return null;
            
            SingletonMaster.Instance.EventManager.NextWaveEvent.Invoke(m_currentWave);
            
            // TODO: This code is shit lol
            foreach (var obstacle in m_allWaveDangers)
            {
                obstacle.SetActive(false);
            }

            if (m_waveCount < m_waveDangers.Count && m_waveCount < m_waveEnvironmentals.Count)
            {
                foreach (var obstacle in m_waveDangers[m_waveCount].m_obstacles)
                {
                    obstacle.SetActive(true);
                }

                // Start moving environmentals
                foreach (var obstacle in m_waveEnvironmentals[m_waveCount].m_envObstacles)
                {
                    Debug.Log("Moving: " + obstacle.gameObject);
                    obstacle.StartMoving();
                }
            }
            else
            {
                Debug.LogError("The wave dangers or wave environmental obstacles are NOT setup properly according to number of waves");
            }
        }
        else
        {
            m_canSpawn = false;
            Debug.LogError("No Waves Setup!!");
        }
    }

    private IEnumerator StartCooldown()
    {
        m_canSpawn = false;
        if (m_waveCount < m_waves.Count - 1)
        {
            foreach (var obstacle in m_allWaveDangers)
            {
                obstacle.SetActive(false);
            }

            if (m_waveCount < m_waveDangers.Count && m_waveCount < m_waveEnvironmentals.Count)
            {
                // Moving back environmentals
                foreach (var obstacle in m_waveEnvironmentals[m_waveCount].m_envObstacles)
                {
                    obstacle.MoveBack();
                }
            }
            else
            {
                Debug.LogError("The wave environmental obstacles are NOT setup properly according to number of waves");
            }

            SingletonMaster.Instance.EventManager.CooldownStarted.Invoke(m_waveCoolDown);
            yield return new WaitForSeconds(m_waveCoolDown);
            
            // New wave
            m_waveCount++;
            m_currentWave = m_waves[m_waveCount];
            m_maxEnemyCount = m_currentWave.m_maxEnemyCount;
            m_waveTime = m_currentWave.m_waveTime;
            
            m_canSpawn = true;
            
            // New Wave
            SingletonMaster.Instance.EventManager.NextWaveEvent.Invoke(m_currentWave);

            if (m_waveCount < m_waveDangers.Count && m_waveCount < m_waveEnvironmentals.Count)
            {
                foreach (var obstacle in m_waveDangers[m_waveCount].m_obstacles)
                {
                    obstacle.SetActive(true);
                }
                
                // Start moving environmentals
                foreach (var obstacle in m_waveEnvironmentals[m_waveCount].m_envObstacles)
                {
                    Debug.Log("Moving: " + obstacle.gameObject);
                    obstacle.StartMoving();
                }
            }
            else
            {
                Debug.LogError("The wave dangers or wave environmental obstacles are NOT setup properly according to number of waves");
            }
        }
        else
        {
            yield return null;
            SingletonMaster.Instance.EventManager.LevelClearEvent.Invoke();
        }
    }

    private void Update()
    {
        if (m_canSpawn)
        {
            m_waveTime -= Time.deltaTime;
            if (m_waveTime <= 0.0f)
            {
                m_waveTime = 0.0f;
                if (m_enemies.Count == 0)
                {
                    StartCoroutine(StartCooldown());
                }
                else
                {
                    SingletonMaster.Instance.EventManager.NeedWaveClearEvent.Invoke();
                }
                // ClearAllEnemies();
            }
            else
            {
                // Spawn enemies
                int enemyCount = m_enemies.Count;
                if (enemyCount < m_maxEnemyCount)
                {
                    SpawnEnemies(m_maxEnemyCount - enemyCount);
                }
            }
        }
    }

    private void ClearAllEnemies()
    {
        for (int i = m_enemies.Count - 1; i >= 0; --i)
        {
            Destroy(m_enemies[i].transform.parent.gameObject);
        }
        m_enemies.Clear();
    }

    private void SpawnEnemies(int num)
    {
        for (int i = 0; i < num; i++)
        {
            Vector2 spawnPos = Camera.main.transform.position;
            spawnPos += GetRandomSpawnPosition();
            
            // Checking spawn security
            RaycastHit2D[] hits = new RaycastHit2D[10];
            int hitNum = Physics2D.CircleCastNonAlloc(spawnPos, 5.0f, Vector2.zero, hits, 0.0f,
                m_maskCheck);
            if (hitNum == 0)
            {
                EnemyScriptable.Enemy newEnemy = m_currentWave.GetRandomEnemyToSpawn();
                GameObject enemyPrefab = newEnemy.m_prefab;
                GameObject spawned = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            
                spawned.transform.GetChild(0).GetComponent<BaseEnemyBehavior>().m_lootDropRate = 
                    newEnemy.m_lootSpawnRate;
                m_enemies.Add(spawned);
            }
        }
    }
    
    // TODO: We can try to spawn enemies inside with some indications too

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
            LootScriptable.LootSpawn loot = SingletonMaster.Instance.LootSpawnScriptableObject.GetRandomLootToSpawn();
            if (loot != null)
            {
                Instantiate(loot.m_prefab, enemy.transform.position, Quaternion.identity);
            }
        }
    }

    private void RemoveEnemy(GameObject enemy)
    {
        // Remove it from enemies
        m_enemies.Remove(enemy.transform.parent.gameObject);

        // Checking for any connected stuff to this enemy
        RopeComponent rc = enemy.GetComponent<RopeComponent>();
        for (int i = rc.m_connectedTo.Count - 1; i >= 0; --i)
        {
            var connectedObj = rc.m_connectedTo[i];
            if (connectedObj)
            {
                connectedObj.GetComponent<RopeComponent>().DetachEnemy(enemy);
            }
        }
        
        // Checking for any received stuff
        for (int i = rc.m_receivedFrom.Count - 1; i >= 0; --i)
        {
            var connector = rc.m_receivedFrom[i];
            if (connector)
            {
                rc.DetachEnemy(connector);
            }
        }
        
        Destroy(enemy.transform.parent.gameObject);
    }
}
