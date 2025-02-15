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
    public GameObject m_playerHealthBar;
    public GameObject m_enemyHealthBarPrefab;
    public GameObject m_durabilityBarPrefab;

    private bool shrinkingTriggered = false; // Ensures shrinking starts only once per wave
    private Coroutine m_cooldown;
    private int m_killCount = 0;
    private float m_waveTime = -1.0f; // Prevents shrinking from starting immediately
    private int m_waveCount = 0;
    private bool waveActive = false; // Tracks if a wave is active

    private Queue<string> announcementQueue = new Queue<string>(); // 🔥 Queue to handle multiple announcements
    private bool announcementActive = false; // 🔥 Prevents messages from overlapping

    private void Start()
    {
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);
        SingletonMaster.Instance.EventManager.CooldownStarted.AddListener(ShowCooldown);
        SingletonMaster.Instance.EventManager.NextWaveEvent.AddListener(OnUpdateWave);
        SingletonMaster.Instance.EventManager.WinEvent.AddListener(OnPlayerWin);
        SingletonMaster.Instance.EventManager.NeedClearEvent.AddListener(OnNeedClear);

        if (SingletonMaster.Instance.PlayerBase != null)
        {
            HealthBar healthBarComp = m_playerHealthBar.GetComponent<HealthBar>();
            healthBarComp.m_healthComp = SingletonMaster.Instance.PlayerBase.gameObject.GetComponent<HealthComponent>();
        }

        Debug.Log("[UI] Waiting for first wave...");
    }

    private void OnNeedClear()
    {
        EnqueueAnnouncement("Kill all leftover enemies", 3.0f);
    }

    private void OnUpdateWave(EnemySpawnScriptable wave)
    {
        Debug.Log($"[UI] New wave started! Wave Time: {wave.m_waveTime}");
        m_waveTime = wave.m_waveTime; // Set wave duration dynamically from EnemyManager
        m_waveCount++;
        shrinkingTriggered = false; // Reset shrinking flag for the new wave
        waveActive = true; // Mark the wave as active
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
        SingletonMaster.Instance.EventManager.CooldownStarted.RemoveListener(ShowCooldown);
        SingletonMaster.Instance.EventManager.NextWaveEvent.RemoveListener(OnUpdateWave);
        SingletonMaster.Instance.EventManager.WinEvent.RemoveListener(OnPlayerWin);
        SingletonMaster.Instance.EventManager.NeedClearEvent.RemoveListener(OnNeedClear);
    }

    private void Update()
    {
        if (waveActive && m_waveTime > 0.0f)
        {
            m_waveTime -= Time.deltaTime;
            if (m_waveTime < 0.0f)
                m_waveTime = 0.0f; // Prevent negative values
        }

        if (waveActive && m_waveTime == 0.0f && !shrinkingTriggered)
        {
            shrinkingTriggered = true;
            waveActive = false; // Prevents this from re-triggering
            StartCoroutine(StartWallShrinkingSequence());
        }

        if (m_waveTime >= 0)
        {
            m_waveText.text = $"Wave Time: {m_waveTime:F2}  Wave Count: {m_waveCount}";
        }
    }

    private IEnumerator StartWallShrinkingSequence()
    {
        Debug.Log("[UI] Wave timer reached 0, preparing to shrink walls...");

        EnqueueAnnouncement("Warning! Walls will start closing in!", 3.0f);
        yield return new WaitForSeconds(3.0f); // Delay before shrinking starts

        EnqueueAnnouncement("The walls are shrinking!", 2.0f);
        Debug.Log("[UI] Triggering Wall Shrinking Event...");
        SingletonMaster.Instance.EventManager.WaveTimeoutEvent.Invoke(); // Notify walls to shrink
    }

    public void EnqueueAnnouncement(string message, float duration = 2.0f)
    {
        announcementQueue.Enqueue(message);
        if (!announcementActive)
        {
            StartCoroutine(DisplayAnnouncement(duration));
        }
    }

    private IEnumerator DisplayAnnouncement(float duration)
    {
        while (announcementQueue.Count > 0)
        {
            announcementActive = true;
            m_announcementText.enabled = true;
            m_announcementText.text = announcementQueue.Dequeue();
            yield return new WaitForSeconds(duration);
        }
        m_announcementText.enabled = false;
        announcementActive = false;
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
        m_cooldown = StartCoroutine(CooldownSequence(time));
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
        m_finalMessageText.text = "YOU DIED!! Press R to restart";
    }

    private void OnPlayerWin()
    {
        m_finalMessageText.enabled = true; 
        m_finalMessageText.text = "YOU WIN!! Press R to restart";
    }
}
