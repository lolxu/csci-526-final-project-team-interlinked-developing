using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability")]
public class AbilityScriptable : ScriptableObject
{
    public string m_name;
    public bool m_enabled;
}
