using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponList", menuName = "ScriptableObjects/WeaponList")]
public class WeaponListScriptable : ScriptableObject
{
    public List<WeaponScriptable> m_weapons = new List<WeaponScriptable>();
}
