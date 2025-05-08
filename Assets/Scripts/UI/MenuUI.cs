using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ScriptableObjects;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [Header("Menu UI Settings")] 
    [SerializeField] private GameObject m_buttonPanel;
    [SerializeField] private LevelDataScriptable m_levelData;

    private void Start()
    {
        m_buttonPanel.transform.localScale = Vector3.zero;
        m_buttonPanel.transform.DOScale(Vector3.one, 2.0f).SetEase(Ease.InOutExpo);
    }

    public void ToggleRangeIndicator(bool val)
    {
        m_levelData.m_needsRopeRangeIndicator = val;
    }
}
