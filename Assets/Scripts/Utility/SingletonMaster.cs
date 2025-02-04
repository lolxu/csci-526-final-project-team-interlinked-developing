using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SingletonMaster : MonoBehaviour
{
    private static SingletonMaster _instance;
    public static SingletonMaster Instance { get { return _instance; } }

    [Header("Game Things")] 
    public PlayerBase PlayerBase;
    public EnemySpawner EnemySpawnerScript;

    [Header("Event Manager")] 
    public EventManager EventManager;

    [Header("Scriptable Objects")] 
    public WeightedRandomScriptable WeightedRandomItems;
    public EnemySpawnScriptable EnemySpawnScriptableObject;

    [Header("Juice Settings")] 
    public CameraShake CameraShakeManager;
    public FeelManager FeelManager;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } 
        else 
        {
            _instance = this;
        }
    }
}
