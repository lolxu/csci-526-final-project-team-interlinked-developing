using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    [Header("Player Events")] 
    public UnityEvent<float, GameObject> PlayerDamageEvent = new UnityEvent<float, GameObject>();
    public UnityEvent<GameObject> PlayerDeathEvent = new UnityEvent<GameObject>();
    
    [Header("Common Enemy Events")]
    public UnityEvent<GameObject> EnemyDeathEvent = new UnityEvent<GameObject>();
}
