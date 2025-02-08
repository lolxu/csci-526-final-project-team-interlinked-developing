using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAbilityList", menuName = "ScriptableObjects/PlayerAbilityList")]
public class PlayerAbilityListScriptable : ScriptableObject
{
    public List<AbilityScriptable> m_abilities = new List<AbilityScriptable>();

    public bool CheckAbilityEnabled(AbilityScriptable ability)
    {
        return ability.m_enabled;
    }
}
