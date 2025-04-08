using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Proyecto26;
using ScriptableObjects;
using Unity.VisualScripting;
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

    [Header("Weapon Settings")] 
    public WeaponListScriptable m_weaponList;

    [Header("Ability Settings")] 
    public PlayerAbilityListScriptable m_abilityList;

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
        
        // For obtaining the number of rope connections & disconnections each level upon completion
        public List<float> m_ropeConnectionMetrics = new List<float>();
        public List<float> m_ropeDisconnectionMetrics = new List<float>();
        
        // For death heat map
        public int m_deathCount = 0;
        public List<SerializableVector2> m_deathLocations = new List<SerializableVector2>();
    }

    [Serializable]
    public class WeaponMetrics
    {
        public string m_weaponName;
        public float m_stealRate;

        public void AddSteal()
        {
            m_stealCount++;
        }

        public void AddSpawn()
        {
            m_spawnCount++;
        }

        public void CalculateRate()
        {
            m_stealRate = (float)m_stealCount / m_spawnCount;
        }

        private int m_spawnCount;
        private int m_stealCount;
    }

    [Serializable]
    public class AbilityMetrics
    {
        public string m_abilityName;
        public float m_activationRate;
        
        public void AddActivation()
        {
            m_activateCount++;
        }

        public void AddSpawn()
        {
            m_spawnCount++;
        }

        public void CalculateRate()
        {
            m_activationRate = (float)m_activateCount / m_spawnCount;
        }
        
        private int m_spawnCount;
        private int m_activateCount;
    }
    
    [Serializable]
    public class MetricsData
    {
        public string m_sessionID;
        public List<LevelMetrics> m_levelMetricsData = new List<LevelMetrics>();
        
        // Weapon Steal Rate
        public List<WeaponMetrics> m_weaponMetricsData = new List<WeaponMetrics>();
        
        // Ability Activation Rate
        public List<AbilityMetrics> m_abilityMetricsData = new List<AbilityMetrics>();
        
        // Death By Enemy

        
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

        public void RecordWeaponSteal(string weaponName)
        {
            if (Instance.m_canRecord)
            {
                foreach (var weaponMetrics in m_weaponMetricsData)
                {
                    if (weaponMetrics.m_weaponName == weaponName)
                    {
                        weaponMetrics.AddSteal();
                        weaponMetrics.CalculateRate();
                        break;
                    }
                }
            }
        }

        public void RecordWeaponSpawn(string weaponName)
        {
            if (Instance.m_canRecord)
            {
                bool found = false;
                foreach (var weaponMetrics in m_weaponMetricsData)
                {
                    if (weaponMetrics.m_weaponName == weaponName)
                    {
                        weaponMetrics.AddSpawn();
                        weaponMetrics.CalculateRate();
                        found = true;
                        break;
                    }
                }

                // If it's a new weapon add to the metrics list
                if (!found)
                {
                    WeaponMetrics weapon = new WeaponMetrics();
                    weapon.m_weaponName = weaponName;
                    weapon.AddSpawn();
                    m_weaponMetricsData.Add(weapon);
                }
            }
        }

        public void RecordAblityActivate(string abilityName)
        {
            if (Instance.m_canRecord)
            {
                foreach (var abilityMetrics in m_abilityMetricsData)
                {
                    if (abilityMetrics.m_abilityName == abilityName)
                    {
                        abilityMetrics.AddActivation();
                        abilityMetrics.CalculateRate();
                        break;
                    }
                }
            }
        }
        
        public void RecordAblitySpawn(string abilityName)
        {
            if (Instance.m_canRecord)
            {
                bool found = false;
                foreach (var abilityMetrics in m_abilityMetricsData)
                {
                    if (abilityMetrics.m_abilityName == abilityName)
                    {
                        abilityMetrics.AddSpawn();
                        abilityMetrics.CalculateRate();
                        found = true;
                        break;
                    }
                }
                
                // If it's a new ability add to the metrics list
                if (!found)
                {
                    AbilityMetrics ability = new AbilityMetrics();
                    ability.m_abilityName = abilityName;
                    ability.AddSpawn();
                    m_abilityMetricsData.Add(ability);
                }
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
                m_levelMetricsData[level].m_deathCount += 1;

                Debug.Log("Death Position: " + position);
                
                // Also dividing the rope operations
                for (int i = 0; i < m_levelMetricsData[level].m_ropeConnectionMetrics.Count; i++)
                {
                    m_levelMetricsData[level].m_ropeConnectionMetrics[i] /= m_levelMetricsData[level].m_deathCount;
                    m_levelMetricsData[level].m_ropeDisconnectionMetrics[i] /= m_levelMetricsData[level].m_deathCount;
                }
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
        SingletonMaster.Instance.EventManager.LevelClearEvent.AddListener(UponLevelCompleted);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.LevelClearEvent.RemoveListener(UponLevelCompleted);
    }

    private void UponLevelCompleted()
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
