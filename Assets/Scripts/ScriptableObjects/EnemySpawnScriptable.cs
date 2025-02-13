using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Wave_0", menuName = "ScriptableObjects/EnemySpawnWave")]
public class EnemySpawnScriptable : ScriptableObject
{
    public List<EnemyScriptable> m_weightedSpawns = new List<EnemyScriptable>();
    public float m_waveTime = 60.0f;
    public int m_maxEnemyCount = 20;
    
    public EnemyScriptable.Enemy GetRandomEnemyToSpawn()
    {
        EnemyScriptable.Enemy output = null;
        
        // Generate random value based on list
        float totalWeight = 0.0f;
        foreach (var weightedItem in m_weightedSpawns)
        {
            totalWeight += weightedItem.enemy.m_weightedValue;
        }
        float randomValue = Random.Range(0.0f, totalWeight);
            
        // Checking where random weight falls
        float processedWeight = 0.0f;
        foreach (var weightedItem in m_weightedSpawns)
        {
            processedWeight += weightedItem.enemy.m_weightedValue;
            if (randomValue <= processedWeight)
            {
                output = weightedItem.enemy.m_enemy;
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
            .OrderBy(item => item.enemy.m_enemy.m_enemyType)
            .ToList();
        
    }
}
