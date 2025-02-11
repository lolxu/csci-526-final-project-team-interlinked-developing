using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability")]
public class AbilityScriptable : ScriptableObject
{
    public string m_name;
    public bool m_enabled = false;
    // THIS IS TEMPORARY!!!!
    public int m_count = 0;

    private void OnEnable()
    {
        m_enabled = false;
        m_count = 0;
    }
}
