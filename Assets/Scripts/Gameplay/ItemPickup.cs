using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ShopManager m_shopManager;
    public TextMeshPro m_text;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    private WeightedRandomScriptable.UpgradeItem m_item;

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
            switch (m_item.m_itemType)
            {
                case WeightedRandomScriptable.ItemType.Bullet:
                {
                    var comp = SingletonMaster.Instance.PlayerBase.gameObject.GetComponent<ShootComponent>();
                    if (comp != null)
                    {
                        comp.m_bulletPrefab = m_item.m_itemPrefab;
                    }
                    break;
                }
                case WeightedRandomScriptable.ItemType.Linkable:
                {
                    // Spawn the prefab
                    Instantiate(m_item.m_itemPrefab, transform.position, Quaternion.identity);
                    break;
                }
            }
            
            // Checking loot count
            SingletonMaster.Instance.LootManager.CheckLoot();
            
        }
    }
}
