using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Proyecto26;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MetricsManager : MonoBehaviour
{
    private static MetricsManager _instance;
    public static MetricsManager Instance { get { return _instance; } }

    public bool m_canRecord = true;
    
    [Header("Google Sheet Settings")]
    [SerializeField] private string m_URL;
    public MetricsData m_metricsData;

    [Header("Level Data")] 
    public LevelDataScriptable m_levelData;
    
    private long m_sessionID;

    [Serializable]
    public class SerializableVector2
    {
        public float x;
        public float y;
    }
    
    [Serializable]
    public class LevelMetrics
    {
        public string m_levelName;
        
        // For obtaining the number of rope connections & disconnections each level
        public List<int> m_ropeConnectionMetrics = new List<int>();
        public List<int> m_ropeDisconnectionMetrics = new List<int>();
        
        // For death heat map
        public List<SerializableVector2> m_deathLocations = new List<SerializableVector2>();
    }
    
    [Serializable]
    public class MetricsData
    {
        public string m_sessionID;
        public List<LevelMetrics> m_levelMetricsData = new List<LevelMetrics>();

        public void Init()
        {
            // Initializing arrays to match with level
            for (int i = 0; i < Instance.m_levelData.m_levelNames.Count; i++)
            {
                LevelMetrics level = new LevelMetrics
                {
                    m_levelName = Instance.m_levelData.m_levelNames[i],
                };

                for (int j = 0; j < Instance.m_levelData.m_waveCount[i]; j++)
                {
                    level.m_ropeConnectionMetrics.Add(0);
                    level.m_ropeDisconnectionMetrics.Add(0);
                }
                
                m_levelMetricsData.Add(level);
            }
        }
        
        public void RecordRopeOperations(int level, int wave, bool isConnection)
        {
            if (Instance.m_canRecord)
            {
                if (isConnection)
                {
                    m_levelMetricsData[level].m_ropeConnectionMetrics[wave] += 1;
                }
                else
                {
                    m_levelMetricsData[level].m_ropeDisconnectionMetrics[wave] += 1;
                }

                Debug.Log("Connection: " + m_levelMetricsData[level].m_ropeConnectionMetrics[wave]);
                Debug.Log("Disconnection: " + m_levelMetricsData[level].m_ropeDisconnectionMetrics[wave]);
            }
        }

        public void RecordDeath(int level, Vector2 position)
        {
            if (Instance.m_canRecord)
            {
                SerializableVector2 deathPos = new SerializableVector2();
                deathPos.x = position.x;
                deathPos.y = position.y;

                m_levelMetricsData[level].m_deathLocations.Add(deathPos);


                Debug.Log("Death Position: " + position);
            }
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        } 
        else 
        {
            _instance = this;
            m_sessionID = DateTime.Now.Ticks;
            
            // Initializing metrics
            if (m_canRecord)
            {
                m_metricsData.Init();
            }
        }
    }

    private void Start()
    {
        
    }

    private void OnApplicationQuit()
    {
        Send();
    }

    public void Send()
    {
        Post(m_sessionID.ToString());
    }

    private void Post(string sessionID)
    {
        if (m_canRecord)
        {
            Debug.Log("Sending data...");
            m_metricsData.m_sessionID = sessionID;

            string serializedData = JsonUtility.ToJson(m_metricsData);
            Debug.Log(serializedData);

            RequestHelper helper = new RequestHelper
            {
                Uri = "https://interlink-metrics-default-rtdb.firebaseio.com/.json",
                Body = m_metricsData
            };

            RestClient.Post(helper);
        }
        /*
        // This is GOOGLE SHEETS --------------------------------------------------

        // Create the form and enter responses
        WWWForm form = new WWWForm();
        form.AddField("entry.593653925", sessionID);
        form.AddField("entry.1567891549", m_metricsData.m_ropeConnectionMetrics[0]);
        form.AddField("entry.566185792", m_metricsData.m_ropeDisconnectionMetrics[0]);
        form.AddField("entry.323290341", m_metricsData.m_ropeConnectionMetrics[1]);
        form.AddField("entry.888263648", m_metricsData.m_ropeDisconnectionMetrics[1]);
        form.AddField("entry.2136525870", m_metricsData.m_ropeConnectionMetrics[2]);
        form.AddField("entry.1012187919", m_metricsData.m_ropeDisconnectionMetrics[2]);

        // Send responses and verify result
        using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to upload: " + www.error);
            }
            else
            {
                Debug.Log("Google Form upload complete!");
            }
        }
        */
    }
}
