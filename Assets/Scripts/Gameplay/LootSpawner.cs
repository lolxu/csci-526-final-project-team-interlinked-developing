using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class LootSpawner : MonoBehaviour
{
    public LootSpawnScriptable m_lootTable;
    public int m_maxLootInArea = 3;
    public float m_spawnTimeoutRange = 5.0f;

    private float m_spawnTimeout = 0.0f;
    private float m_spawnTimer = 0.0f;
    private bool m_isSpawning = false;
    [SerializeField] private List<GameObject> m_spawnedLoot = new List<GameObject>();
    private BoxCollider2D m_collider;

    private void Start()
    {
        m_collider = GetComponent<BoxCollider2D>();

        m_spawnTimeout = Random.Range(2.0f, m_spawnTimeoutRange);
    }

    private void Update()
    {
        if (!m_isSpawning)
        {
            m_spawnTimer += Time.deltaTime;

            if (m_spawnTimer >= m_spawnTimeout && m_spawnedLoot.Count < m_maxLootInArea)
            {
                m_spawnTimeout = Random.Range(2.0f, m_spawnTimeoutRange);
                m_spawnTimer = 0.0f;
                SpawnLoot();
            }
        }
    }

    private void SpawnLoot()
    {
        LootScriptable.LootSpawn spawn = m_lootTable.GetRandomLootToSpawn();
        Vector2 position = GetRandomSpawnPosition();
        GameObject spawnedObj = Instantiate(spawn.m_prefab, position, Quaternion.identity);
        Vector3 scale = spawnedObj.transform.localScale;
        spawnedObj.transform.localScale = Vector3.zero;
        spawnedObj.layer = 0;
        
        spawnedObj.transform.DOScale(scale, 0.15f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            spawnedObj.layer = SingletonMaster.Instance.CONNECTABLE_LAYER;
            m_spawnedLoot.Add(spawnedObj);
        });
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 pos = transform.position;
        float randX = Random.Range(-m_collider.size.x / 2.0f, m_collider.size.x / 2.0f);
        float randY = Random.Range(-m_collider.size.y / 2.0f, m_collider.size.y / 2.0f);
        Vector2 randomPos = pos + new Vector2(randX, randY);
        return randomPos;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject target = other.gameObject;
        if (m_spawnedLoot.Contains(target))
        {
            m_spawnedLoot.Remove(target);
        }
    }
}
