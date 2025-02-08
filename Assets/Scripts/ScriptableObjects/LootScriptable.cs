using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loot", menuName = "ScriptableObjects/Loot")]
public class LootScriptable : ScriptableObject
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
    public struct RandomLoot
    {
        public LootSpawn m_loot;
        
        [Range(0.0f, 1.0f)]
        public float m_weightedValue;
    }

    public RandomLoot loot;
}
