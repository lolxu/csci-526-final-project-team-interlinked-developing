using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LootManager : MonoBehaviour
{
    [Header("Loot Settings")] 
    public int m_currentLootCount = 0;
    public int m_goalLoot = 5;

    public bool m_isFirstSpawn = true;
    // public AnimationCurve m_lootIncreaseCurve;

    private void Start()
    {
        SingletonMaster.Instance.EventManager.LootCollected.AddListener(AddLoot);
        
    }

    private void Update()
    {
        // Spawn all items the first time
        if (m_isFirstSpawn)
        {
            SingletonMaster.Instance.EventManager.ItemSpawnEvent.Invoke();
            m_isFirstSpawn = false;
        }
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
