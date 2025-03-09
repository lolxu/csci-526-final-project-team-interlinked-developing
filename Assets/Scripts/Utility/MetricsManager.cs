using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class MetricsManager : MonoBehaviour
{
    private static MetricsManager _instance;
    public static MetricsManager Instance { get { return _instance; } }
    
    [Header("Google Sheet Settings")]
    [SerializeField] private string m_URL;
    public MetricsData m_metricsData;

    [Header("Level Data")] 
    public LevelDataScriptable m_levelData;
    
    [DllImport("__Internal")]
    private static extern void Initialize();
    
    private long m_sessionID;
    
    [Serializable]
    public class MetricsData
    {
        // For obtaining the number of rope connections & disconnections each level
        public List<int> m_ropeConnectionMetrics = new List<int>();
        public List<int> m_ropeDisconnectionMetrics = new List<int>();

        public void Init()
        {
            // Initializing arrays to match with level
            for (int i = 0; i < Instance.m_levelData.m_levelNames.Count; i++)
            {
                m_ropeConnectionMetrics.Add(0);
                m_ropeDisconnectionMetrics.Add(0);
            }
        }
        
        public void RecordRopeOperations(int level, bool isConnection)
        {
            if (isConnection)
            {
                m_ropeConnectionMetrics[level] += 1;
            }
            else
            {
                m_ropeDisconnectionMetrics[level] += 1;
            }

            Debug.Log("Connection: " + m_ropeConnectionMetrics[level]);
            Debug.Log("Disconnection: " + m_ropeDisconnectionMetrics[level]);
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
            m_metricsData.Init();
        }
    }

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        Initialize();
#endif
    }

    public void Send()
    {
        Debug.Log("Sending data...");
        StartCoroutine(Post(m_sessionID.ToString()));
    }

    private IEnumerator Post(string sessionID)
    {
        // Create the form and enter responses
        WWWForm form = new WWWForm();
        form.AddField("entry.862385992", sessionID);
        form.AddField("entry.665573761", m_metricsData.m_ropeConnectionMetrics[0]);
        form.AddField("entry.1224369195", m_metricsData.m_ropeDisconnectionMetrics[0]);
        
        // Send responses and verify result
        using (UnityWebRequest www = UnityWebRequest.Post(m_URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Google Form upload complete!");
            }
        }
    }
}
