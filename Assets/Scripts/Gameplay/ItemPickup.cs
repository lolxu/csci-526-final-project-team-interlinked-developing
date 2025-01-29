using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public TextMeshPro m_text;
    private SpriteRenderer m_spriteRenderer;
    private WeightedRandomScriptable.UpgradeItem m_item;
    
    private void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_item = SingletonMaster.Instance.WeightedRandomItems.GetRandomItem();

        m_spriteRenderer.sprite = m_item.m_itemIcon;
        m_text.text = m_item.m_itemName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
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
            }

            Destroy(gameObject);
        }
    }
}
