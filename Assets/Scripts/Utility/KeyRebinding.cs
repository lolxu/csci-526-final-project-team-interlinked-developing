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
    [SerializeField] private GameObject m_bindingWarning;

    private InputActionRebindingExtensions.RebindingOperation m_rebindOp;
    private Coroutine m_warningCoroutine;

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

        m_action.action.Disable();
        
        m_rebindOp = m_action.action.PerformInteractiveRebinding()
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                int bindIdx = m_action.action.GetBindingIndexForControl(m_action.action.controls[0]);

                if (CheckDuplicateBindings(m_action.action, bindIdx))
                {
                    if (m_warningCoroutine != null)
                    {
                        StopCoroutine(m_warningCoroutine);
                    }
                    m_warningCoroutine = StartCoroutine(ShowErrorMessage());
                    
                    m_action.action.RemoveBindingOverride(bindIdx);
                    m_bindingDisplayText.text =
                        InputControlPath.ToHumanReadableString(m_action.action.bindings[bindIdx].effectivePath);
                    m_rebindOp.Dispose();
                    m_startRebindObject.SetActive(true);
                    m_waitingForInputObject.SetActive(false);
                    return;
                }
                
                m_bindingDisplayText.text =
                    InputControlPath.ToHumanReadableString(m_action.action.bindings[bindIdx].effectivePath);
                m_rebindOp.Dispose();
                
                m_action.action.Enable();
                m_startRebindObject.SetActive(true);
                m_waitingForInputObject.SetActive(false);
                
                // SingletonMaster.Instance.PlayerBase.m_input.SwitchCurrentActionMap("Gameplay");
            })
            .Start();
    }

    private bool CheckDuplicateBindings(InputAction action, int bindingIndex, bool allCompositeParts = false)
    {
        InputBinding newBinding = action.bindings[bindingIndex];
        foreach (var binding in action.actionMap.bindings)
        {
            if (binding.action == newBinding.action)
            {
                continue;
            }

            if (binding.effectivePath == newBinding.effectivePath)
            {
                Debug.LogError("Duplicate Bindings!");
                return true;
            }
        }

        if (allCompositeParts)
        {
            for (int i = 1; i < bindingIndex; i++)
            {
                if (action.bindings[i].effectivePath == newBinding.overridePath)
                {
                    Debug.LogError("Duplicate Bindings!");
                    return true;
                }
            }
        }

        return false;
    }

    IEnumerator ShowErrorMessage()
    {
        m_bindingWarning.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        m_bindingWarning.SetActive(false);
    }
}
