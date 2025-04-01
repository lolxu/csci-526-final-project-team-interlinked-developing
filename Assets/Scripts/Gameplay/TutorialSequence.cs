using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class TutorialSequence : MonoBehaviour
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

    [Header("Walls - Dope Visuals")]
    public GameObject m_upWall;
    public GameObject m_downWall;
    public GameObject m_leftWall;
    public GameObject m_rightWall;
    
    private TutorialProgress m_currentStep = TutorialProgress.Moving;
    private int m_abilityCount = 0;
    private int m_combatKill = 0;

    private void Start()
    {
        if (GameManager.Instance.m_levelData.m_needsTutorial)
        {
            SingletonMaster.Instance.EventManager.TutorialPlayerMoved.AddListener(OnPlayerMoved);
            SingletonMaster.Instance.EventManager.TutorialPlayerKilledEnemy.AddListener(OnPlayerKilledEnemy);
            SingletonMaster.Instance.EventManager.TutorialPlayerAbility.AddListener(OnPlayerAbility);
            m_controlPrompts[0].SetActive(true);
        }
        else
        {
            m_currentStep = TutorialProgress.Done;
            TransitionIntoGameplay();
        }
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.TutorialPlayerMoved.RemoveListener(OnPlayerMoved);
        SingletonMaster.Instance.EventManager.TutorialPlayerKilledEnemy.RemoveListener(OnPlayerKilledEnemy);
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
    
    private void OnPlayerKilledEnemy()
    {
        if (m_currentStep == TutorialProgress.RopeOperations)
        {
            m_currentStep = TutorialProgress.Ability;

            StartCoroutine(TransitionToAbility());
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
        Vector3 orgScale = enemy.transform.localScale;
        enemy.transform.localScale = Vector3.zero;
        enemy.transform.DOScale(orgScale, 0.5f).SetEase(Ease.InOutSine);
    }
    
    private IEnumerator TransitionToAbility()
    {
        yield return null;
        
        m_controlPrompts[1].SetActive(false);
        m_controlPrompts[2].SetActive(true);
        
        m_abilityObj.SetActive(true);
    }

    private void TransitionToCombat()
    {
        m_controlPrompts[2].SetActive(false);
        m_controlPrompts[3].SetActive(true);
        
        for (int i = 0; i < m_combatEnemies.Count; i++)
        {
            GameObject enemy = Instantiate(m_combatEnemies[i], m_combatSpawns[i].position, Quaternion.identity);
            Vector3 orgScale = enemy.transform.localScale;
            enemy.transform.localScale = Vector3.zero;
            enemy.transform.DOScale(orgScale, 0.5f).SetEase(Ease.InOutSine);
        }
    }

    private void TransitionIntoGameplay()
    {
        m_controlPrompts[3].SetActive(false);
        
        GameManager.Instance.m_levelData.m_needsTutorial = false;
        
        // Move Walls & Show Title
        SingletonMaster.Instance.EventManager.TutorialDone.Invoke();
        SingletonMaster.Instance.UI.ShowBigText("INTERLINK", 5.0f);
        
        Sequence wallSeq = DOTween.Sequence();
        wallSeq.Insert(0, m_upWall.transform.DOMoveY(100.0f, 5.0f).SetEase(Ease.InOutSine));
        wallSeq.Insert(0, m_downWall.transform.DOMoveY(-100.0f, 5.0f).SetEase(Ease.InOutSine));
        wallSeq.Insert(0, m_leftWall.transform.DOMoveX(-100.0f, 5.0f).SetEase(Ease.InOutSine));
        wallSeq.Insert(0, m_rightWall.transform.DOMoveX(100.0f, 5.0f).SetEase(Ease.InOutSine));

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
}
