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
    public TextMeshProUGUI m_scoreText;
    public TextMeshProUGUI m_coolDownText;
    public GameObject m_playerHealthBar;
    public GameObject m_enemyHealthBarPrefab;
    public GameObject m_durabilityBarPrefab;

    private Coroutine m_cooldown;
    private int m_killCount = 0;
    
    private void Start()
    {
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);
        SingletonMaster.Instance.EventManager.CooldownStarted.AddListener(ShowCooldown);
        SingletonMaster.Instance.EventManager.WinEvent.AddListener(OnPlayerWin);
        
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            HealthBar healthBarComp = m_playerHealthBar.GetComponent<HealthBar>();
            healthBarComp.m_healthComp = SingletonMaster.Instance.PlayerBase.gameObject.GetComponent<HealthComponent>();
        }
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
        SingletonMaster.Instance.EventManager.CooldownStarted.RemoveListener(ShowCooldown);
        SingletonMaster.Instance.EventManager.WinEvent.RemoveListener(OnPlayerWin);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == SingletonMaster.Instance.HubName)
        {
            if (m_cooldown != null)
            {
                StopCoroutine(m_cooldown);
                m_coolDownText.enabled = false;
            }
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
        m_coolDownText.enabled = true;
        m_cooldown = StartCoroutine(CooldownSequence(time));
    }

    private IEnumerator CooldownSequence(float time)
    {
        while (time > 0.0f)
        {
            m_coolDownText.text = "Cool down: " + time.ToString("#");
            yield return null;
            time -= Time.deltaTime;
        }
        m_coolDownText.enabled = false;
    }

    private void AddKill(GameObject enemy)
    {
        m_killCount++;
        m_scoreText.text = m_killCount.ToString();
    }

    private void OnPlayerDeath(GameObject killer)
    {
        // m_scoreText.enabled = true;
        // BaseEnemyBehavior enemy = killer.GetComponent<BaseEnemyBehavior>();
        // string name = enemy.m_names[Random.Range(0, enemy.m_names.Count)];
        // m_scoreText.text = "Killed by " + name;
    }

    private void OnPlayerWin()
    {
        m_scoreText.enabled = true; 
        m_scoreText.text = "YOU WIN!!";
    }
}
