using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ShopManager m_shopManager;
    public TextMeshPro m_text;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    private UpgradeItemScriptable.UpgradeItem m_item;

    public void SpawnItem()
    {
        m_item = SingletonMaster.Instance.WeightedRandomItems.GetRandomItem();

        m_spriteRenderer.sprite = m_item.m_itemIcon;
        m_text.text = m_item.m_itemName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject.GetComponent<PlayerBase>() != null)
        {
            Instantiate(m_item.m_itemPrefab, transform.position, Quaternion.identity);
            
            // Checking loot count
            SingletonMaster.Instance.LootManager.m_hasCollected = true;
            SingletonMaster.Instance.LootManager.m_goalLoot += Random.Range(15, 20);
            SingletonMaster.Instance.LootManager.CheckLoot();
            
        }
    }
}
