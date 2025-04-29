using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

public class ToggleCheat : MonoBehaviour
{
    [SerializeField] private LevelDataScriptable m_levelData;

    public void UnlockAllLevels()
    {
        m_levelData.UnlockAllLevels();
    }
}
