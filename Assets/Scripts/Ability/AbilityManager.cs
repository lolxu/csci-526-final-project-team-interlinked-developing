using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityManager : MonoBehaviour
{
    public enum AbilityTypes
    {
        Dash,
        Invincible
    }
    
    public UnityEvent<AbilityTypes> ActivateAbility = new UnityEvent<AbilityTypes>();
}
