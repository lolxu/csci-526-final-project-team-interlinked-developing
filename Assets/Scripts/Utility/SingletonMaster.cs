using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SingletonMaster : MonoBehaviour
{
    private static SingletonMaster _instance;
    public static SingletonMaster Instance { get { return _instance; } }

    [Header("Game Things")] 
    public PlayerBase PlayerBase;
    public EnemyManager EnemyManager;
    public LootManager LootManager;

    [Header("Event Manager")] 
    public EventManager EventManager;

    [Header("Scriptable Objects")] 
    public WeightedRandomScriptable WeightedRandomItems;
    public EnemySpawnScriptable EnemySpawnScriptableObject;

    [Header("Juice Settings")] 
    public CameraShake CameraShakeManager;
    public FeelManager FeelManager;
    
    [Header("Constants")]
    public int UNCONNECTED_LAYER = 6;
    public int PLAYER_LAYER = 7;

    [Header("Scene Names")] 
    public string HubName = "Prototype - Hub";
    public string BattlefieldName = "Prototype - Battlefield";
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } 
        else 
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
