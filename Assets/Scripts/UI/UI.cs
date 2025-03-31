using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI m_finalMessageText;
    public TextMeshProUGUI m_waveText;
    public TextMeshProUGUI m_announcementText;
    public TextMeshProUGUI m_ropeCountText;
    public GameObject m_playerHealthBar;
    public GameObject m_enemyHealthBarPrefab;
    public GameObject m_durabilityBarPrefab;

    // private bool shrinkingTriggered = false; // Ensures shrinking starts only once per wave
    private int m_killCount = 0;
    private float m_waveTime = -1.0f; // Prevents shrinking from starting immediately
    private int m_waveCount = 0;
    private bool waveActive = false; // Tracks if a wave is active

    private Queue<string> m_announcementQueue = new Queue<string>(); // Queue to handle multiple announcements
    private bool m_announcementActive = false; // Prevents messages from overlapping
    private Coroutine m_announcement;

    private int m_maxConnections = 0;

    private void Start()
    {
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);
        SingletonMaster.Instance.EventManager.CooldownStarted.AddListener(ShowCooldown);
        SingletonMaster.Instance.EventManager.NextWaveEvent.AddListener(OnUpdateWave);
        SingletonMaster.Instance.EventManager.LevelClearEvent.AddListener(OnLevelClear);
        SingletonMaster.Instance.EventManager.NeedWaveClearEvent.AddListener(OnNeedClear);
        SingletonMaster.Instance.EventManager.PlayerWinEvent.AddListener(OnWin);

        if (SingletonMaster.Instance.PlayerBase != null)
        {
            HealthBar healthBarComp = m_playerHealthBar.GetComponent<HealthBar>();
            healthBarComp.m_healthComp = SingletonMaster.Instance.PlayerBase.gameObject.GetComponent<HealthComponent>();

            m_maxConnections = SingletonMaster.Instance.PlayerBase.m_maxRopeConnections;
        }
    }

    private void OnNeedClear()
    {
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            // Using the old version for now
            m_announcementText.enabled = true;
            m_announcementText.text = "Kill all leftover enemies";
        }
    }

    private void OnUpdateWave(EnemySpawnScriptable wave)
    {
        Debug.Log($"New wave started! Wave Time: {wave.m_waveTime}");
        m_waveTime = wave.m_waveTime; // Set wave duration dynamically from EnemyManager
        m_waveCount++;
        // shrinkingTriggered = false; // Reset shrinking flag for the new wave
        waveActive = true; // Mark the wave as active
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
        SingletonMaster.Instance.EventManager.CooldownStarted.RemoveListener(ShowCooldown);
        SingletonMaster.Instance.EventManager.NextWaveEvent.RemoveListener(OnUpdateWave);
        SingletonMaster.Instance.EventManager.LevelClearEvent.RemoveListener(OnLevelClear);
        SingletonMaster.Instance.EventManager.NeedWaveClearEvent.RemoveListener(OnNeedClear);
        SingletonMaster.Instance.EventManager.PlayerWinEvent.RemoveListener(OnWin);
    }

    private void Update()
    {
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            m_waveTime -= Time.deltaTime;
            if (m_waveTime < 0.0f)
            {
                m_waveTime = 0.0f; // Prevent negative values
            }
            
            m_waveText.text = $"Wave Time: {m_waveTime:F2}  Wave Count: {m_waveCount}";
            m_ropeCountText.text = "Rope: " + (SingletonMaster.Instance.PlayerBase.m_linkedObjects.Count - 1) + "/" +
                                   m_maxConnections;
        }
        else
        {
            m_waveText.text = $"Wave Time: You Died...  Wave Count: {m_waveCount}";
            m_ropeCountText.text = "Rope: DEAD/" + m_maxConnections;
        }
    }


    public GameObject AddHealthBar(HealthComponent healthComponent)
    {
        Debug.Log("Added health bar: " + healthComponent.gameObject);
        if (healthComponent.gameObject.CompareTag("Enemy"))
        {
            GameObject newHealthBar = Instantiate(m_enemyHealthBarPrefab, transform, true);
            HealthBar healthBarComp = newHealthBar.GetComponent<HealthBar>();
            healthBarComp.m_healthComp = healthComponent;
            return newHealthBar;
        }
        return null;
    }

    public GameObject AddDurabilityBar(DurabilityComponent durability)
    {
        GameObject newDurabilityBar = Instantiate(m_durabilityBarPrefab, transform, true);
        DurabilityBar durabilityBarComp = newDurabilityBar.GetComponent<DurabilityBar>();
        durabilityBarComp.m_durabilityComp = durability;
        return newDurabilityBar;
    }

    private void ShowCooldown(float time)
    {
        StartCoroutine(CooldownSequence(time));
    }

    private IEnumerator CooldownSequence(float time)
    {
        while (time > 0.0f)
        {
            m_announcementText.text = "Cool down: " + time.ToString("F2");
            yield return null;
            time -= Time.deltaTime;
        }
        m_announcementText.enabled = false;
    }

    private void AddKill(GameObject enemy)
    {
        m_killCount++;
        m_finalMessageText.text = m_killCount.ToString();
    }

    private void OnPlayerDeath(GameObject killer)
    {
        m_finalMessageText.enabled = true;
        m_finalMessageText.text = "DEAD";
        m_announcementText.enabled = true;
        m_announcementText.text = "Press R to Restart";
    }

    private void OnLevelClear()
    {
        m_finalMessageText.enabled = true; 
        m_finalMessageText.text = "SURVIVED";
        m_announcementText.enabled = true;
        m_announcementText.text = "Press Space to Continue";
    }
    
    private void OnWin()
    {
        m_finalMessageText.enabled = true; 
        m_finalMessageText.text = "YOU WIN";
        m_announcementText.enabled = true;
        m_announcementText.text = "End of Alpha!";
    }
}
