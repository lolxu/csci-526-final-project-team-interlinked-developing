using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "WeightedRandomItemsList_SO", menuName = "ScriptableObjects/WeightedRandomItemsList", order = 1)]
public class WeightedRandomScriptable : ScriptableObject
{
    public enum Rarity
    {
        Common,
        Rare,
        VeryRare,
        ExtremelyRare,
        Exotic
    }

    public enum ItemType
    {
        Bullet,
        StatsUpgrade,
        RuleUpgrade,
        Linkable
    }
    
    [Serializable]
    public class UpgradeItem
    {
        public string m_itemName;
        public int m_id;
        public Sprite m_itemIcon;
        public ItemType m_itemType;
        public Rarity m_rarity;
        public float m_price;
        public string m_description;
        public GameObject m_itemPrefab;
    }
    
    [Serializable]
    public struct WeightedItem
    {
        public UpgradeItem m_item;
        
        [Range(0.0f, 1.0f)]
        public float m_weightedValue;
    }
    public List<WeightedItem> m_weightedList = new List<WeightedItem>();
    
    private HashSet<int> m_idLookup = new HashSet<int>();
    
    public UpgradeItem GetRandomItem()
    {
        UpgradeItem output = null;
        
        // Generate random value based on list
        float totalWeight = 0.0f;
        foreach (var weightedItem in m_weightedList)
        {
            totalWeight += weightedItem.m_weightedValue;
        }
        float randomValue = Random.Range(0.0f, totalWeight);
            
        // Checking where random weight falls
        float processedWeight = 0.0f;
        foreach (var weightedItem in m_weightedList)
        {
            processedWeight += weightedItem.m_weightedValue;
            if (randomValue <= processedWeight)
            {
                output = weightedItem.m_item;
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
            .OrderBy(item => item.m_item.m_rarity)
            .ThenBy(item => item.m_item.m_id)
            .ToList();
        
        // Checking for double ID entries
        bool check = false;
        int id = -1;
        foreach (var weightedItem in m_weightedList)
        {
            if (!m_idLookup.Contains(weightedItem.m_item.m_id))
            {
                m_idLookup.Add(weightedItem.m_item.m_id);
            }
            else
            {
                check = true;
                id = weightedItem.m_item.m_id;
                break;
            }
        }

        m_idLookup.Clear();
        if (check)
        {
            Debug.LogError("Double entry ID: " + id);
            throw new Exception("There is a double entry in ID for the item list! Please check again!");
        }
    }
}
