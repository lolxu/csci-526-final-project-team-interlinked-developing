using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LootManager : MonoBehaviour
{
    [Header("Loot Settings")] 
    public int m_currentLootCount = 0;
    public int m_goalLoot = 10;
    // public AnimationCurve m_lootIncreaseCurve;

    private void Start()
    {
        SingletonMaster.Instance.EventManager.LootCollected.AddListener(AddLoot);
    }

    private void AddLoot()
    {
        m_currentLootCount++;

        if (m_currentLootCount == m_goalLoot)
        {
            SingletonMaster.Instance.EventManager.ItemSpawnEvent.Invoke();
            m_goalLoot += Random.Range(10, 20);
        }
    }
}
