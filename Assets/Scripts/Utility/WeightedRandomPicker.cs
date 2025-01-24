using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeightedRandomPicker : MonoBehaviour
{
    public WeightedRandomScriptable m_weightedRandom_SO;
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string result = m_weightedRandom_SO.GetRandomValue();
            if (result != null)
            {
                Debug.Log(result);
            }
            else
            {
                Debug.LogError("Something bad happened with random picker...");
            }
        }
    }

    
}
