using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class EventManager : MonoBehaviour
{
    [Header("Player Events")] 
    public UnityEvent<GameObject> PlayerDeathEvent = new UnityEvent<GameObject>();
    public UnityEvent<int> LootCollected = new UnityEvent<int>();
    public UnityEvent StartFireEvent = new UnityEvent();
    public UnityEvent StopFireEvent = new UnityEvent();
    public UnityEvent<float> CooldownStarted = new UnityEvent<float>();
    public UnityEvent<GameObject> LinkEvent = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> UnlinkEvent = new UnityEvent<GameObject>();
    
    [Header("Common Enemy Events")]
    public UnityEvent<GameObject> EnemyDeathEvent = new UnityEvent<GameObject>();
}
