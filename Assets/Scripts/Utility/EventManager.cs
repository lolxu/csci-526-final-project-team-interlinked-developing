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
    public UnityEvent WinEvent = new UnityEvent();
    
    [Header("Connection Events")]
    public UnityEvent<GameObject, GameObject> LinkEvent = new UnityEvent<GameObject, GameObject>();
    public UnityEvent<GameObject, GameObject> UnlinkEvent = new UnityEvent<GameObject, GameObject>();
    public UnityEvent<GameObject, GameObject> StealStartedEvent = new UnityEvent<GameObject, GameObject>();
    public UnityEvent<GameObject, GameObject> StealEndedEvent = new UnityEvent<GameObject, GameObject>();
    public UnityEvent<GameObject, GameObject> StealSuccessEvent = new UnityEvent<GameObject, GameObject>();

    [Header("Common Enemy Events")] 
    public UnityEvent<EnemySpawnScriptable> NextWaveEvent = new UnityEvent<EnemySpawnScriptable>();
    public UnityEvent NeedClearEvent = new UnityEvent();
    public UnityEvent<GameObject> EnemyDeathEvent = new UnityEvent<GameObject>(); 
}
