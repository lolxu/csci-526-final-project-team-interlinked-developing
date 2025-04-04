using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

public class KnockBackAbility : MonoBehaviour
{
    [SerializeField] private AbilityScriptable m_ability;
    [SerializeField] private AbilityManager.AbilityTypes m_abilityType;
    [SerializeField] private LayerMask m_knockBackMask;
    [SerializeField] private float m_knockBackRadius = 10.0f;
    [SerializeField] private float m_knockBackStrength = 100.0f;

    private bool m_canActivate = true;
    
    private void Start()
    {
        SingletonMaster.Instance.AbilityManager.ActivateAbility.AddListener(OnAbilityActivated);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.AbilityManager.ActivateAbility.RemoveListener(OnAbilityActivated);
    }

    private void OnAbilityActivated(AbilityManager.AbilityTypes type)
    {
        if (type == m_abilityType && m_canActivate && m_ability.CheckAbilityEnabled())
        {
            Debug.Log("Ability Activated");
            m_canActivate = false;
            Knockback();
            StartCoroutine(CoolDown());
        }
    }

    private void Knockback()
    {
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            Vector2 playerPos = SingletonMaster.Instance.PlayerBase.transform.position;

            RaycastHit2D[] hits = Physics2D.CircleCastAll(playerPos, m_knockBackRadius, Vector2.zero, 0.0f, m_knockBackMask);
            foreach (var hit in hits)
            {
                hit.rigidbody.AddExplosionForce(m_knockBackStrength, playerPos, m_knockBackRadius);
            }
            
            SingletonMaster.Instance.AbilityManager.AbilityFinished.Invoke(m_abilityType);
        }
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(m_ability.m_coolDown);
        m_canActivate = true;
    }
}
