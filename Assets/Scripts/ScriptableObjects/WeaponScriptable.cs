using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon")]
public class WeaponScriptable : ScriptableObject
{
    public enum WeaponType
    {
        Gun,
        Melee
    }
    
    public string m_name;
    public WeaponType m_type;
    public float m_physicalDamage;
    public float m_physicalDamageVelocityThreshold;
}
