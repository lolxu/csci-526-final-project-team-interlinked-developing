using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "WeightedRandomItemsList_SO", menuName = "ScriptableObjects/WeightedRandomItemsList")]
public class WeightedRandomScriptable : ScriptableObject
{
    public List<UpgradeItemScriptable> m_weightedList = new List<UpgradeItemScriptable>();
    
    public UpgradeItemScriptable.UpgradeItem GetRandomItem()
    {
        UpgradeItemScriptable.UpgradeItem output = null;
        
        // Generate random value based on list
        float totalWeight = 0.0f;
        foreach (var weightedItem in m_weightedList)
        {
            totalWeight += weightedItem.upgradeItem.m_weightedValue;
        }
        float randomValue = Random.Range(0.0f, totalWeight);
            
        // Checking where random weight falls
        float processedWeight = 0.0f;
        foreach (var weightedItem in m_weightedList)
        {
            processedWeight += weightedItem.upgradeItem.m_weightedValue;
            if (randomValue <= processedWeight)
            {
                output = weightedItem.upgradeItem.m_item;
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
        // Sort list based on rarity
        m_weightedList = m_weightedList
            .OrderBy(item => item.upgradeItem.m_item.m_rarity)
            .ToList();
    }
}
