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

    public void SetAbility(AbilityScriptable ability, bool status)
    {
        foreach (var ab in m_abilities)
        {
            if (ab == ability)
            {
                ability.m_enabled = status;
                break;
            }
        }
    }

    public void ResetAbilities()
    {
        foreach (var ability in m_abilities)
        {
            ability.m_enabled = false;
        }
    }
}
