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
    public TextMeshProUGUI m_announcementText;
    public TextMeshProUGUI m_ropeCountText;
    public GameObject m_playerHealthBar;
    public GameObject m_enemyHealthBarPrefab;
    public GameObject m_durabilityBarPrefab;
    public GameObject m_worldSpaceUIParent;
    public GameObject m_pauseMenu;
    public GameObject m_waveProgress;
    public Image m_waveFill;

    // private bool shrinkingTriggered = false; // Ensures shrinking starts only once per wave
    private int m_killCount = 0;
    private float m_waveTime = 0.0f;
    private int m_waveCount = 0;
    private float m_totalWaveTime = 0.0f;
    private bool waveActive = false; // Tracks if a wave is active

    private Queue<string> m_announcementQueue = new Queue<string>(); // Queue to handle multiple announcements
    private bool m_announcementActive = false; // Prevents messages from overlapping
    private Coroutine m_announcement;

    private int m_maxConnections = 0;
    private bool m_isPlayerDead = false;

    private bool m_canIncreaseProgress = false;

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
            
            SetupWaveUI();
        }
    }

    private void OnNeedClear()
    {
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            m_canIncreaseProgress = false;
            // Using the old version for now
            m_announcementText.enabled = true;
            m_announcementText.text = "Kill all leftover enemies";
        }
    }

    private void OnUpdateWave(EnemySpawnScriptable wave)
    {
        if (!m_waveProgress.activeInHierarchy)
        {
            m_canIncreaseProgress = true;
            m_waveProgress.SetActive(true);
        }
        
        Debug.Log($"New wave started! Wave Time: {wave.m_waveTime}");
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
            if (m_canIncreaseProgress)
            {
                m_waveTime += Time.deltaTime;
            }

            m_waveFill.fillAmount = m_waveTime / m_totalWaveTime;
            
            // m_waveText.text = $"Wave Time: {m_waveTime:F2}  Wave Count: {m_waveCount}";
            m_ropeCountText.text = "ROPE: " + (SingletonMaster.Instance.PlayerBase.m_linkedObjects.Count - 1) + "/" +
                                   m_maxConnections;
        }
        else
        {
            // m_waveText.text = $"Wave Time: You Died...  Wave Count: {m_waveCount}";
            m_ropeCountText.text = "ROPE: DEAD/" + m_maxConnections;
        }
    }


    public GameObject AddHealthBar(HealthComponent healthComponent, float offset = -1.0f)
    {
        Debug.Log("Added health bar: " + healthComponent.gameObject);
        if (healthComponent.gameObject.CompareTag("Enemy"))
        {
            GameObject newHealthBar = Instantiate(m_enemyHealthBarPrefab, m_worldSpaceUIParent.transform, true);
            HealthBar healthBarComp = newHealthBar.GetComponent<HealthBar>();
            healthBarComp.m_offset.y = offset;
            healthBarComp.m_healthComp = healthComponent;
            return newHealthBar;
        }
        return null;
    }

    public GameObject AddDurabilityBar(DurabilityComponent durability)
    {
        GameObject newDurabilityBar = Instantiate(m_durabilityBarPrefab, m_worldSpaceUIParent.transform, true);
        DurabilityBar durabilityBarComp = newDurabilityBar.GetComponent<DurabilityBar>();
        durabilityBarComp.m_durabilityComp = durability;
        return newDurabilityBar;
    }

    private void ShowCooldown(float time)
    {
        if (!m_isPlayerDead)
        {
            m_canIncreaseProgress = false;
            StartCoroutine(CooldownSequence(time));
        }
    }

    private IEnumerator CooldownSequence(float time)
    {
        while (time > 0.0f)
        {
            m_announcementText.text = "Cool Down, Healing: " + time.ToString("F2");
            yield return null;
            time -= Time.deltaTime;
        }
        m_announcementText.enabled = false;
        m_canIncreaseProgress = true;
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

        m_isPlayerDead = true;
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
        m_announcementText.text = "End of Demo (for now...)";
    }

    public void ShowBigText(string words, float duration)
    {
        StartCoroutine(ShowBigTextCoroutine(words, duration));
    }

    private IEnumerator ShowBigTextCoroutine(string words, float duration)
    {
        m_finalMessageText.enabled = true;
        m_finalMessageText.text = words;

        yield return new WaitForSeconds(duration);
        
        m_finalMessageText.enabled = false;
    }

    public void PauseGame()
    {
        SingletonMaster.Instance.AudioManager.PlayUISFX("UIPauseMenu");
        m_pauseMenu.SetActive(true);
        Time.timeScale = 0.0f;
        // SingletonMaster.Instance.AudioManager.PauseAudio(true);
    }

    public void ResumeGame()
    {
        m_pauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
        // SingletonMaster.Instance.AudioManager.PauseAudio(false);
        GameManager.Instance.m_gamePaused = false;
    }

    private void SetupWaveUI()
    {
        foreach (var wave in SingletonMaster.Instance.WaveManager.m_waves)
        {
            m_totalWaveTime += wave.m_waveTime;
        }
    }
}
