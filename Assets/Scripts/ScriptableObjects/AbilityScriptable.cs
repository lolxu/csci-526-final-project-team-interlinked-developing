using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability")]
public class AbilityScriptable : ScriptableObject
{
    public string m_name;
    public float m_activeDuration = 0.25f;
    public float m_coolDown = 1.0f;

    public int m_count = 0;

    public void AddLink()
    {
        m_count++;
    }

    public void RemoveLink()
    {
        m_count--;
    }

    private void OnEnable()
    {
        m_count = 0;
    }

    public void ResetAbility()
    {
        m_count = 0;
    }

    public bool CheckAbilityEnabled()
    {
        return m_count > 0;
    }
}
