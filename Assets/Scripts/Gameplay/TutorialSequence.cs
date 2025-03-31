using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialSequence : MonoBehaviour
{
    private enum TutorialProgress
    {
        None,
        Moving,
        RopeOperations,
        Ability
    }
    
    public List<GameObject> m_controlPrompts = new List<GameObject>();
    private TutorialProgress m_currentStep = TutorialProgress.None;

    private void Start()
    {
        SingletonMaster.Instance.EventManager.PlayerMoved.AddListener(OnPlayerMoved);
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnPlayerConnected);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnPlayerDisconnected);
        SingletonMaster.Instance.AbilityManager.ActivateAbility.AddListener(OnPlayerAbilityActivated);
        
        m_controlPrompts[0].SetActive(true);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.PlayerMoved.RemoveListener(OnPlayerMoved);
        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnPlayerConnected);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnPlayerDisconnected);
        SingletonMaster.Instance.AbilityManager.ActivateAbility.RemoveListener(OnPlayerAbilityActivated);
    }

    public void OnPlayerMoved()
    {
        if (m_currentStep == TutorialProgress.None)
        {
            m_currentStep = TutorialProgress.Moving;
            
            // Showing next prompt
            m_controlPrompts[0].SetActive(false);
            m_controlPrompts[1].SetActive(true);
        }
    }
    
    public void OnPlayerConnected(GameObject obj, GameObject instigator)
    {
        
    }
    
    public void OnPlayerDisconnected(GameObject obj, GameObject instigator)
    {
        
    }
    
    public void OnPlayerAbilityActivated(AbilityManager.AbilityTypes type)
    {
        
    }

    private void TransitionIntoGameplay()
    {
        
    }
}
