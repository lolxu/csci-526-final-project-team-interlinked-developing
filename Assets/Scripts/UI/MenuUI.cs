using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [Header("Menu UI Settings")] 
    [SerializeField] private GameObject m_buttonPanel;

    private void Start()
    {
        m_buttonPanel.transform.localScale = Vector3.zero;
        m_buttonPanel.transform.DOScale(Vector3.one, 2.0f).SetEase(Ease.InOutExpo);
    }
}
