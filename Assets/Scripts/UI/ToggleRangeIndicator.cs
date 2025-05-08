using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

public class ToggleRangeIndicator : MonoBehaviour
{
    [SerializeField] private Toggle m_toggle;
    [SerializeField] private LevelDataScriptable m_levelData;

    private void OnEnable()
    {
        m_toggle.isOn = m_levelData.m_needsRopeRangeIndicator;
    }
}
