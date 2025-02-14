using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI m_finalMessageText;
    public TextMeshProUGUI m_waveText;
    public TextMeshProUGUI m_announcementText;
    public GameObject m_playerHealthBar;
    public GameObject m_enemyHealthBarPrefab;
    public GameObject m_durabilityBarPrefab;

    private Coroutine m_cooldown;
    private int m_killCount = 0;
    private float m_waveTime = 0.0f;
    private int m_waveCount = 0;
    
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
    }

    private void OnNeedClear()
    {
        m_announcementText.enabled = true;
        m_announcementText.text = "Kill all leftover enemies";
    }

    private void OnUpdateWave(EnemySpawnScriptable wave)
    {
        m_waveTime = wave.m_waveTime;
        m_waveCount++;
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
        m_waveTime -= Time.deltaTime;
        if (m_waveTime <= 0.0f)
        {
            m_waveTime = 0.0f;
        }
        Debug.Log("wave time: " + m_waveTime.ToString("F2") + "        " + "wave count: " + m_waveCount);
        m_waveText.text = "wave time: " + m_waveTime.ToString("F2") + "        " + "wave count: " + m_waveCount;
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
