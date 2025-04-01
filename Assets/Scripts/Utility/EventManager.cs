using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    public UnityEvent LevelClearEvent = new UnityEvent();
    public UnityEvent PlayerWinEvent = new UnityEvent();
    
    [Header("Connection Events")]
    // Object, Instigator
    public UnityEvent<GameObject, GameObject> LinkEvent = new UnityEvent<GameObject, GameObject>();
    public UnityEvent<GameObject, GameObject> UnlinkEvent = new UnityEvent<GameObject, GameObject>();
    
    // Item, Enemy
    public UnityEvent<GameObject, GameObject> StealStartedEvent = new UnityEvent<GameObject, GameObject>();
    public UnityEvent<GameObject, GameObject> StealEndedEvent = new UnityEvent<GameObject, GameObject>();
    public UnityEvent<GameObject, GameObject> StealSuccessEvent = new UnityEvent<GameObject, GameObject>();

    [Header("Common Enemy Events")] 
    public UnityEvent<EnemySpawnScriptable> NextWaveEvent = new UnityEvent<EnemySpawnScriptable>();
    public UnityEvent NeedWaveClearEvent = new UnityEvent();
    
    // The targeted enemy is passed through
    public UnityEvent<GameObject> EnemyDeathEvent = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> EnemyDamagedEvent = new UnityEvent<GameObject>();
    
    public UnityEvent WaveTimeoutEvent = new UnityEvent();

    [Header("Tutorial Events")] 
    public UnityEvent TutorialPlayerMoved = new UnityEvent();
    public UnityEvent TutorialPlayerKilledEnemy = new UnityEvent();
    public UnityEvent TutorialPlayerAbility = new UnityEvent();
    public UnityEvent TutorialKillAll = new UnityEvent();
    public UnityEvent TutorialDone = new UnityEvent();
    
}
