using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DummyEnemyBehavior : BaseEnemyBehavior
{
    [Header("Dummy Enemy Stuff")] 
    [SerializeField] private GameObject m_connectPrompt;
    [SerializeField] private GameObject m_disconnectPrompt;

    public bool m_canShowPrompt = true;
    private bool m_isPromptDone = false;
    
    protected override void Start()
    {
        base.Start();
        
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        
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
        }
    }
    
    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject && !m_isPromptDone && m_canShowPrompt)
        {
            m_disconnectPrompt.SetActive(false);
            m_isPromptDone = true;
        }
    }

}
