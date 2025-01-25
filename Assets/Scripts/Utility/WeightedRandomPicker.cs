using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeightedRandomPicker : MonoBehaviour
{
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            WeightedRandomScriptable.UpgradeItem result = SingletonMaster.Instance.WeightedRandomItems.GetRandomItem();
            Debug.Log(result.m_itemName);
        }
    }

    
}
