using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "EnemySpawn_SO", menuName = "ScriptableObjects/EnemySpawn", order = 1)]
public class EnemySpawnScriptable : ScriptableObject
{
    [Serializable]
    public class Enemy
    {
        public enum EnemyType
        {
            Basic,
            Brute,
            Stealer,
            Shooter,
            Shield,
            Spawner,
            Teleporter
        }

        public GameObject m_prefab;
        public EnemyType m_enemyType;
    }

    [Serializable]
    public struct WeightedEnemySpawn
    {
        public Enemy m_enemy;
        
        [Range(0.0f, 1.0f)]
        public float m_weightedValue;
    }
    
    public List<WeightedEnemySpawn> m_weightedSpawns = new List<WeightedEnemySpawn>();
    
    public Enemy GetRandomEnemyToSpawn()
    {
        Enemy output = null;
        
        // Generate random value based on list
        float totalWeight = 0.0f;
        foreach (var weightedItem in m_weightedSpawns)
        {
            totalWeight += weightedItem.m_weightedValue;
        }
        float randomValue = Random.Range(0.0f, totalWeight);
            
        // Checking where random weight falls
        float processedWeight = 0.0f;
        foreach (var weightedItem in m_weightedSpawns)
        {
            processedWeight += weightedItem.m_weightedValue;
            if (randomValue <= processedWeight)
            {
                output = weightedItem.m_enemy;
                break;
            }
        }

        if (output == null)
        {
            Debug.LogError("Something bad happened with random picker");
        }

        return output;
    }
    
    private void OnValidate()
    {
        // Sort list based on Enemy Type
        m_weightedSpawns = m_weightedSpawns
            .OrderBy(item => item.m_enemy.m_enemyType)
            .ToList();
        
    }
}
