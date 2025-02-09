using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public TextMeshPro m_text;
    public GameObject m_itemsHolder;
    public List<GameObject> m_items = new List<GameObject>();
    private Coroutine m_shopRoutine = null;
    
    private void Update()
    {
        int currentLoot = SingletonMaster.Instance.LootManager.m_currentLootCount;
        int goalLoot = SingletonMaster.Instance.LootManager.m_goalLoot;
        m_text.text = "Loot: " + currentLoot + "/" + goalLoot;
    }

    public void EnableShopItems()
    {
        if (m_shopRoutine != null)
        {
            StopCoroutine(m_shopRoutine);
            m_itemsHolder.SetActive(false);
        }
        
        m_shopRoutine = StartCoroutine(RollForShop());
    }

    private IEnumerator RollForShop()
    {
        m_itemsHolder.SetActive(false);
        foreach (var item in m_items)
        {
            item.GetComponent<ItemPickup>().SpawnItem();
        }
        yield return new WaitForSeconds(0.5f);
        m_itemsHolder.SetActive(true);
        
    }

    public void DisableShopItems()
    {
        m_itemsHolder.SetActive(false);
    }
}
