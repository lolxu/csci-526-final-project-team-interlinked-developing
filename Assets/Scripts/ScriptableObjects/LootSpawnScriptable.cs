using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "LootSpawn_SO", menuName = "ScriptableObjects/LootSpawn")]
public class LootSpawnScriptable : ScriptableObject
{
    public List<LootScriptable> m_weightedSpawns = new List<LootScriptable>();
    
    public LootScriptable.LootSpawn GetRandomLootToSpawn()
    {
        LootScriptable.LootSpawn output = null;
        
        // Generate random value based on list
        float totalWeight = 0.0f;
        foreach (var weightedItem in m_weightedSpawns)
        {
            totalWeight += weightedItem.loot.m_weightedValue;
        }
        float randomValue = Random.Range(0.0f, totalWeight);
            
        // Checking where random weight falls
        float processedWeight = 0.0f;
        foreach (var weightedItem in m_weightedSpawns)
        {
            processedWeight += weightedItem.loot.m_weightedValue;
            if (randomValue <= processedWeight)
            {
                output = weightedItem.loot.m_loot;
                break;
            }
        }

        if (output == null)
        {
            Debug.Log("Whoops, no loot spawn this time lol");
        }

        return output;
    }
    
    private void OnValidate()
    {
        // Sort list based on Enemy Type
        m_weightedSpawns = m_weightedSpawns
            .OrderBy(item => item.loot.m_loot.m_lootType)
            .ToList();
        
    }
}
