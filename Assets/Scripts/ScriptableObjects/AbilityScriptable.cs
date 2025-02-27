using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability")]
public class AbilityScriptable : ScriptableObject
{
    public string m_name;
    public bool m_enabled = false;
    public float m_activeDuration = 0.25f;
    public float m_coolDown = 1.0f;

    private void OnEnable()
    {
        m_enabled = false;
    }
}
