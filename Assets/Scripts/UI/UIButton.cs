using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    private Button m_button;

    private void Start()
    {
        m_button = GetComponent<Button>();
        m_button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        SingletonMaster.Instance.AudioManager.PlayUISFX("UIButtonClick");
    }
}
