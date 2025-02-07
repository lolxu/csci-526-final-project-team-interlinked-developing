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

    public bool m_isFirstCollected = false;
    // public AnimationCurve m_lootIncreaseCurve;

    private void Start()
    {
        SingletonMaster.Instance.EventManager.LootCollected.AddListener(AddLoot);
        SingletonMaster.Instance.EventManager.ItemCollected.AddListener(CollectedItem);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LootCollected.RemoveListener(AddLoot);
        SingletonMaster.Instance.EventManager.ItemCollected.RemoveListener(CollectedItem);
    }

    private void CollectedItem()
    {
        m_isFirstCollected = true;
    }

    private void Update()
    {
        // Spawn all items the first time
        if (m_currentLootCount == 0 && !m_isFirstCollected)
        {
            SingletonMaster.Instance.EventManager.ItemSpawnEvent.Invoke();
        }
    }

    private void AddLoot(int value)
    {
        m_currentLootCount += value;

        if (m_currentLootCount >= m_goalLoot)
        {
            SingletonMaster.Instance.EventManager.ItemSpawnEvent.Invoke();
            m_goalLoot += Random.Range(15, 20);
        }
    }
}
