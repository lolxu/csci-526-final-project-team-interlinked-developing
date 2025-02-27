using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

public class DashAbility : MonoBehaviour
{
    [SerializeField] private AbilityScriptable m_ability;
    [SerializeField] private LayerMask m_dashMasks;
    [SerializeField] private AbilityManager.AbilityTypes m_abilityType;
    [SerializeField] private float m_dashForceMult = 1000.0f;

    private bool m_canActivate = true;
    
    private void Start()
    {
        SingletonMaster.Instance.AbilityManager.ActivateAbility.AddListener(OnAbilityActivated);
    }

    private void OnAbilityActivated(AbilityManager.AbilityTypes type)
    {
        if (type == m_abilityType && m_canActivate && m_ability.m_enabled)
        {
            Debug.Log("Ability Activated");
            m_canActivate = false;
            StartCoroutine(Dash());
            StartCoroutine(CoolDown());
        }
    }

    private IEnumerator Dash()
    {
        GameObject player = SingletonMaster.Instance.PlayerBase.gameObject;
        Vector2 dashDir = SingletonMaster.Instance.PlayerBase.m_moveDirection;
        
        player.GetComponent<Collider2D>().excludeLayers = m_dashMasks;
        Debug.Log(dashDir * m_dashForceMult);
        SingletonMaster.Instance.PlayerBase.m_isDashing = true;
        Color orgColor = player.GetComponent<SpriteRenderer>().color;
        Color newColor = orgColor;
        newColor.a = 0.25f;
        player.GetComponent<SpriteRenderer>().color = newColor;
        player.GetComponent<Rigidbody2D>().AddForce(dashDir * m_dashForceMult, ForceMode2D.Impulse);

        yield return new WaitForSeconds(m_ability.m_activeDuration);
        
        player.GetComponent<Rigidbody2D>().totalForce = Vector2.zero;
        SingletonMaster.Instance.PlayerBase.m_isDashing = false;
        player.GetComponent<Collider2D>().excludeLayers = default;
        player.GetComponent<SpriteRenderer>().color = orgColor;
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(m_ability.m_coolDown);
        m_canActivate = true;
    }
}
