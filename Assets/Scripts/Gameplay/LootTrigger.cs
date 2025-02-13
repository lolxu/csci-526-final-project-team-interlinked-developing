using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootTrigger : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Loot") && other.gameObject.layer == SingletonMaster.Instance.UNCONNECTED_LAYER)
        {
            other.gameObject.GetComponent<HealthPickup>().StartShrinking();
        }
    }
}
