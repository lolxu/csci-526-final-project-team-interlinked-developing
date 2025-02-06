using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI m_scoreText;
    public TextMeshProUGUI m_coolDownText;
    public GameObject m_healthBarPrefab;

    private int m_killCount = 0;
    
    private void Start()
    {
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);
        SingletonMaster.Instance.EventManager.CooldownStarted.AddListener(ShowCooldown);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
        SingletonMaster.Instance.EventManager.CooldownStarted.RemoveListener(ShowCooldown);
    }

    public GameObject AddHealthBar(HealthComponent healthComponent)
    {
        Debug.Log("Added health bar: " + healthComponent.gameObject);
        GameObject newHealthBar = Instantiate(m_healthBarPrefab, transform, true);
        HealthBar healthBarComp = newHealthBar.GetComponent<HealthBar>();
        healthBarComp.m_healthComp = healthComponent;
        return newHealthBar;
    }

    private void ShowCooldown(float time)
    {
        m_coolDownText.enabled = true;
        StartCoroutine(CooldownSequence(time));
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
        m_scoreText.enabled = true;
        BaseEnemyBehavior enemy = killer.GetComponent<BaseEnemyBehavior>();
        string name = enemy.m_names[Random.Range(0, enemy.m_names.Count)];
        m_scoreText.text = "Killed by " + name;
    }
}
