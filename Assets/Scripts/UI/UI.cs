using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI m_scoreText;
    public TextMeshProUGUI m_coolDownText;
    
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
