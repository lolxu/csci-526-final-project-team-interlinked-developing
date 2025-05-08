using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityManager : MonoBehaviour
{
    public enum AbilityTypes
    {
        Dash,
        Knockback
    }
    
    public UnityEvent<AbilityTypes> ActivateAbility = new UnityEvent<AbilityTypes>();
    public UnityEvent<AbilityTypes> AbilityFinished = new UnityEvent<AbilityTypes>();
}
