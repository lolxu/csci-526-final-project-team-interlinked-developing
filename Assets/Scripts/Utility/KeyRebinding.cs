using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyRebinding : MonoBehaviour
{
    [SerializeField] private InputActionReference m_action;
    [SerializeField] private TMP_Text m_bindingDisplayText;
    [SerializeField] private GameObject m_startRebindObject;
    [SerializeField] private GameObject m_waitingForInputObject;

    private InputActionRebindingExtensions.RebindingOperation m_rebindOp;

    private void Start()
    {
        m_bindingDisplayText.text =
            InputControlPath.ToHumanReadableString(m_action.action.bindings[0].effectivePath);
    }

    public void StartRebinding()
    {
        m_startRebindObject.SetActive(false);
        m_waitingForInputObject.SetActive(true);
        
        // SingletonMaster.Instance.PlayerBase.m_input.SwitchCurrentActionMap("UI");

        m_rebindOp = m_action.action.PerformInteractiveRebinding()
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                int bindIdx = m_action.action.GetBindingIndexForControl(m_action.action.controls[0]);
                
                m_bindingDisplayText.text =
                    InputControlPath.ToHumanReadableString(m_action.action.bindings[bindIdx].effectivePath);
                
                m_rebindOp.Dispose();
                
                m_startRebindObject.SetActive(true);
                m_waitingForInputObject.SetActive(false);
                
                // SingletonMaster.Instance.PlayerBase.m_input.SwitchCurrentActionMap("Gameplay");
            })
            .Start();
    }
    
}
