using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SingletonMaster : MonoBehaviour
{
    private static SingletonMaster _instance;
    public static SingletonMaster Instance { get { return _instance; } }

    [Header("Player Things")] 
    public PlayerControl PlayerController;

    [Header("Scriptable Objects")] 
    public WeightedRandomScriptable WeightedRandomItems;

    [Header("Feel Manager Settings")] 
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
