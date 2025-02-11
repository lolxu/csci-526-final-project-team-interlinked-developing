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
    public int m_goalLoot = 0;
    public bool m_hasCollected = false;

    private void Start()
    {
        SingletonMaster.Instance.EventManager.LootCollected.AddListener(AddLoot);
        GameObject.FindWithTag("Shop").GetComponent<ShopManager>().EnableShopItems();
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LootCollected.RemoveListener(AddLoot);
    }

    private void AddLoot(int value)
    {
        m_currentLootCount += value;
        CheckLoot();
    }

    public void CheckLoot()
    {
        if (m_hasCollected)
        {
            if (m_currentLootCount >= m_goalLoot)
            {
                Debug.Log("Can Spawn Items " + m_currentLootCount + "/" + m_goalLoot);
                GameObject.FindWithTag("Shop").GetComponent<ShopManager>().EnableShopItems();
                m_hasCollected = false;
            }
            else
            {
                GameObject.FindWithTag("Shop").GetComponent<ShopManager>().DisableShopItems();
                Debug.Log("Cannot Spawn!!!!!! Not enough loot");
            }
        }
    }
}
