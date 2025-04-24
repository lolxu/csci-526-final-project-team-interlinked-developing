using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class DummyEnemyBehavior : BaseEnemyBehavior
{
    [Header("Dummy Enemy Stuff")] 
    [SerializeField] private InputActionReference m_connectAction;
    [SerializeField] private InputActionReference m_disconnectAction;
    [SerializeField] private GameObject m_connectPrompt;
    [SerializeField] private TMP_Text m_connectText;
    [SerializeField] private GameObject m_disconnectPrompt;
    [SerializeField] private TMP_Text m_disconnectText;
    
    public bool m_canShowPrompt = true;
    private bool m_isPromptDone = false;
    
    protected override void Start()
    {
        base.Start();
        
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);

        if (m_canShowPrompt)
        {
            m_healthComponent.m_canDamage = false;
        }
        
        m_connectText.text = InputControlPath.ToHumanReadableString(m_connectAction.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        m_disconnectText.text = InputControlPath.ToHumanReadableString(m_disconnectAction.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    private void OnDestroy()
    {
        SingletonMaster.Instance.EventManager.TutorialPlayerKilledEnemy.Invoke();
        
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && !m_isPromptDone && m_canShowPrompt) 
        {
            m_connectPrompt.SetActive(false);
            m_disconnectPrompt.SetActive(true);

            SingletonMaster.Instance.EventManager.TutorialLinkedEnemy.Invoke();
        }
    }
    
    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && !m_isPromptDone && m_canShowPrompt)
        {
            m_disconnectPrompt.SetActive(false);
            m_isPromptDone = true;

            m_healthComponent.m_canDamage = true;
            
            SingletonMaster.Instance.EventManager.TutorialUnlinkedEnemy.Invoke();
        }
    }

}
