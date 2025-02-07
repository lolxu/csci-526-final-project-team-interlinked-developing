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

    private void Update()
    {
        int currentLoot = SingletonMaster.Instance.LootManager.m_currentLootCount;
        int goalLoot = SingletonMaster.Instance.LootManager.m_goalLoot;
        m_text.text = "Loot: " + currentLoot + "/" + goalLoot;
    }

    public void EnableShopItems()
    {
        StartCoroutine(RollForShop());
    }

    private IEnumerator RollForShop()
    {
        m_itemsHolder.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        m_itemsHolder.SetActive(true);
        foreach (var item in m_items)
        {
            item.GetComponent<ItemPickup>().SpawnItem();
        }
    }

    public void DisableShopItems()
    {
        m_itemsHolder.SetActive(false);
    }
}
