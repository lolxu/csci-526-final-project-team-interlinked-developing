using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
    private GameObject SingletonMasterObject;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        } 
        else 
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Attaching Singleton Master to itself to be not destroyed
        if (transform.childCount == 0)
        {
            SingletonMasterObject = GameObject.Find("Singleton Master");
            SingletonMasterObject.transform.SetParent(transform, true);
        }
    }

    private void Start()
    {
        
        
    }

    private void Update()
    {
        // FOR DEBUG ONLY
        if (Input.GetKeyDown(KeyCode.R))
        {
            SingletonMasterObject.transform.SetParent(GameObject.FindWithTag("Background").transform, true);
            SceneManager.LoadScene("Prototype - Hub");
        }
    }
}
