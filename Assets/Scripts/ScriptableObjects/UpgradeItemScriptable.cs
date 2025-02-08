using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeItem", menuName = "ScriptableObjects/UpgradeItem")]
public class UpgradeItemScriptable : ScriptableObject
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
        StatsUpgrade,
        ControlModule,
        Turrets,
        Melee,
        Defense,
        Collection
    }
    
    [Serializable]
    public class UpgradeItem
    {
        public string m_itemName;
        public Sprite m_itemIcon;
        public ItemType m_itemType;
        public Rarity m_rarity;
        public float m_price;
        public string m_description;
        public GameObject m_itemPrefab;
    }
    
    [Serializable]
    public struct RandomUpgradeItem
    {
        public UpgradeItem m_item;
        
        [Range(0.0f, 1.0f)]
        public float m_weightedValue;
    }

    public RandomUpgradeItem upgradeItem;
}
