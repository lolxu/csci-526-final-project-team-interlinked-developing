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
    public LootManager LootManager;
    public UI UI;

    [Header("Event Manager")] 
    public EventManager EventManager;

    [Header("Scriptable Objects")] 
    public WeightedRandomScriptable WeightedRandomItems;
    public LootSpawnScriptable LootSpawnScriptableObject;
    public PlayerAbilityListScriptable PlayerAbilities;

    [Header("Juice Settings")] 
    public FeelManager FeelManager;
    public CameraShake CameraShakeManager;
    
    [Header("Constants")]
    public int UNCONNECTED_LAYER = 6;
    public int PLAYER_LAYER = 7;

    [Header("Scene Names")] 
    public string HubName = "Prototype - Hub";
    public string BattlefieldName = "Prototype - Battlefield";

    private bool m_restart = false;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        } 
        else 
        {
            _instance = this;
        }
    }
}
