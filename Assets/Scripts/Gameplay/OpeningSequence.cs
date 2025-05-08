using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class OpeningSequence : MonoBehaviour
{
    private enum TutorialProgress
    {
        Moving,
        RopeOperations,
        Ability,
        Combat,
        Done
    }
    
    [Header("Tutorial Sequence Settings")]
    public List<GameObject> m_controlPrompts = new List<GameObject>();
    public GameObject m_dummyEnemy;
    public Transform m_dummySpawn;
    public List<GameObject> m_combatEnemies;
    public List<Transform> m_combatSpawns;
    public GameObject m_abilityObj;

    [Header("Opening Dope Visuals")]
    public GameObject m_upWall;
    public GameObject m_downWall;
    public GameObject m_leftWall;
    public GameObject m_rightWall;
    public string m_levelName;
    
    private TutorialProgress m_currentStep = TutorialProgress.Moving;
    private int m_abilityCount = 0;
    private int m_combatKill = 0;

    private void Start()
    {
        if (GameManager.Instance.m_levelData.m_needsTutorial)
        {
            SingletonMaster.Instance.EventManager.TutorialPlayerMoved.AddListener(OnPlayerMoved);
            SingletonMaster.Instance.EventManager.TutorialLinkedEnemy.AddListener(OnDummyLinked);
            SingletonMaster.Instance.EventManager.TutorialUnlinkedEnemy.AddListener(OnDummyUnlinked);
            SingletonMaster.Instance.EventManager.TutorialPlayerKilledEnemy.AddListener(OnPlayerKilledEnemy);
            SingletonMaster.Instance.EventManager.TutorialPlayerLinkedAbility.AddListener(OnPlayerLinkedAbility);
            SingletonMaster.Instance.EventManager.TutorialPlayerAbility.AddListener(OnPlayerAbility);
            m_controlPrompts[0].SetActive(true);
            
            m_upWall.SetActive(true);
            m_downWall.SetActive(true);
            m_leftWall.SetActive(true);
            m_rightWall.SetActive(true);
        }
        else if (GameManager.Instance.m_levelData.m_needsLevelName)
        {
            m_upWall.SetActive(true);
            m_downWall.SetActive(true);
            m_leftWall.SetActive(true);
            m_rightWall.SetActive(true);
            
            m_currentStep = TutorialProgress.Done;
            ShowLevelName();
        }
        else
        {
            m_currentStep = TutorialProgress.Done;
            StartGameplayImmediately();
        }
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.TutorialPlayerMoved.RemoveListener(OnPlayerMoved);
        SingletonMaster.Instance.EventManager.TutorialLinkedEnemy.RemoveListener(OnDummyLinked);
        SingletonMaster.Instance.EventManager.TutorialUnlinkedEnemy.RemoveListener(OnDummyUnlinked);
        SingletonMaster.Instance.EventManager.TutorialPlayerKilledEnemy.RemoveListener(OnPlayerKilledEnemy);
        SingletonMaster.Instance.EventManager.TutorialPlayerLinkedAbility.RemoveListener(OnPlayerLinkedAbility);
        SingletonMaster.Instance.EventManager.TutorialPlayerAbility.RemoveListener(OnPlayerAbility);
    }

    public void OnPlayerMoved()
    {
        if (m_currentStep == TutorialProgress.Moving)
        {
            m_currentStep = TutorialProgress.RopeOperations;
            
            // To next step
            StartCoroutine(TransitionToRopeOperations());
        }
    }
    
    private void OnDummyLinked()
    {
        if (m_currentStep == TutorialProgress.RopeOperations)
        {
            m_controlPrompts[1].SetActive(false);
            m_controlPrompts[2].SetActive(true);
        }
    }
    
    private void OnDummyUnlinked()
    {
        if (m_currentStep == TutorialProgress.RopeOperations)
        {
            m_controlPrompts[2].SetActive(false);
            m_controlPrompts[3].SetActive(true);
        }
    }
    
    private void OnPlayerKilledEnemy()
    {
        if (m_currentStep == TutorialProgress.RopeOperations)
        {
            m_currentStep = TutorialProgress.Ability;
            m_controlPrompts[3].SetActive(false);
            m_abilityObj.SetActive(true);
        }
        else if (m_currentStep == TutorialProgress.Combat)
        {
            m_combatKill++;
            if (m_combatKill == m_combatEnemies.Count)
            {
                m_currentStep = TutorialProgress.Done;
                
                TransitionIntoGameplay();
            }
        }
    }
    
    private void OnPlayerLinkedAbility()
    {
        if (m_currentStep == TutorialProgress.Ability)
        {
            TransitionToAbility();
        }
    }
    
    private void OnPlayerAbility()
    {
        if (m_currentStep == TutorialProgress.Ability)
        {
            m_abilityCount++;

            if (m_abilityCount == 3)
            {
                m_currentStep = TutorialProgress.Combat;
                TransitionToCombat();
            }
        }
    }

    private IEnumerator TransitionToRopeOperations()
    {
        yield return new WaitForSeconds(3.0f);
        
        m_controlPrompts[0].SetActive(false);
        m_controlPrompts[1].SetActive(true);
        
        // Spawning dummy enemy
        GameObject enemy = Instantiate(m_dummyEnemy, m_dummySpawn.position, Quaternion.identity);
        // Vector3 orgScale = enemy.transform.localScale;
        // enemy.transform.localScale = Vector3.zero;
        // enemy.transform.DOScale(orgScale, 0.5f).SetEase(Ease.InOutSine);
    }
    
    private void TransitionToAbility()
    {
        m_controlPrompts[4].SetActive(true);
    }

    private void TransitionToCombat()
    {
        m_controlPrompts[4].SetActive(false);
        m_controlPrompts[5].SetActive(true);
        
        for (int i = 0; i < m_combatEnemies.Count; i++)
        {
            GameObject enemy = Instantiate(m_combatEnemies[i], m_combatSpawns[i].position, Quaternion.identity);
        }
    }

    private void TransitionIntoGameplay()
    {
        m_controlPrompts[5].SetActive(false);
        
        GameManager.Instance.m_levelData.m_needsTutorial = false;
        
        // Move Walls & Show Title
        SingletonMaster.Instance.EventManager.TutorialDone.Invoke();
        StartCoroutine(TitleDisplay());
        
        Sequence wallSeq = DOTween.Sequence();
        wallSeq.Insert(0, m_upWall.transform.DOMoveY(100.0f, 5.0f).SetEase(Ease.InOutSine));
        wallSeq.Insert(0, m_downWall.transform.DOMoveY(-100.0f, 5.0f).SetEase(Ease.InOutSine));
        wallSeq.Insert(0, m_leftWall.transform.DOMoveX(-100.0f, 5.0f).SetEase(Ease.InOutSine));
        wallSeq.Insert(0, m_rightWall.transform.DOMoveX(100.0f, 5.0f).SetEase(Ease.InOutSine));

        AbilityComponent ac = m_abilityObj.GetComponent<AbilityComponent>();
        ac.ForceDropAbility();
        
        wallSeq.OnComplete(() =>
        {
            m_upWall.SetActive(false);
            m_downWall.SetActive(false);
            m_leftWall.SetActive(false);
            m_rightWall.SetActive(false);
            
            SingletonMaster.Instance.WaveManager.StartWaves();
            
            gameObject.SetActive(false);
        });
    }

    private IEnumerator TitleDisplay()
    {
        SingletonMaster.Instance.UI.ShowBigText("INTERLINK", 2.0f);
        yield return new WaitForSeconds(2.0f);
        SingletonMaster.Instance.UI.ShowBigText(m_levelName, 3.0f);

        // Don't show level name sequence again for this level
        GameManager.Instance.m_levelData.m_needsLevelName = false;
    }

    private void ShowLevelName()
    {
        SingletonMaster.Instance.UI.ShowBigText(m_levelName, 3.0f);
        
        Sequence wallSeq = DOTween.Sequence();
        wallSeq.Insert(0, m_upWall.transform.DOMoveY(100.0f, 3.0f).SetEase(Ease.InOutSine));
        wallSeq.Insert(0, m_downWall.transform.DOMoveY(-100.0f, 3.0f).SetEase(Ease.InOutSine));
        wallSeq.Insert(0, m_leftWall.transform.DOMoveX(-100.0f, 3.0f).SetEase(Ease.InOutSine));
        wallSeq.Insert(0, m_rightWall.transform.DOMoveX(100.0f, 3.0f).SetEase(Ease.InOutSine));

        AbilityComponent ac = m_abilityObj.GetComponent<AbilityComponent>();
        ac.ForceDropAbility();
        
        wallSeq.OnComplete(() =>
        {
            m_upWall.SetActive(false);
            m_downWall.SetActive(false);
            m_leftWall.SetActive(false);
            m_rightWall.SetActive(false);
            
            SingletonMaster.Instance.WaveManager.StartWaves();
            
            gameObject.SetActive(false);
        });
        
        // Don't show level name sequence again for this level
        GameManager.Instance.m_levelData.m_needsLevelName = false;
    }

    private void StartGameplayImmediately()
    {
        m_upWall.SetActive(false);
        m_downWall.SetActive(false);
        m_leftWall.SetActive(false);
        m_rightWall.SetActive(false);
            
        SingletonMaster.Instance.WaveManager.StartWaves();
            
        gameObject.SetActive(false);
    }
}
