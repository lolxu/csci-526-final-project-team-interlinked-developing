using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "LootSpawn_SO", menuName = "ScriptableObjects/LootSpawn", order = 1)]
public class LootSpawnScriptable : ScriptableObject
{
    [Serializable]
    public class LootSpawn
    {
        public enum LootType
        {
            Basic,
            Environmental
        }

        public GameObject m_prefab;
        public LootType m_lootType;
    }

    [Serializable]
    public struct WeightedLootSpawn
    {
        public LootSpawn m_loot;
        
        [Range(0.0f, 1.0f)]
        public float m_weightedValue;
    }
    
    public List<WeightedLootSpawn> m_weightedSpawns = new List<WeightedLootSpawn>();
    
    public LootSpawn GetRandomLootToSpawn()
    {
        LootSpawn output = null;
        
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
                output = weightedItem.m_loot;
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
            .OrderBy(item => item.m_loot.m_lootType)
            .ToList();
        
    }
}
