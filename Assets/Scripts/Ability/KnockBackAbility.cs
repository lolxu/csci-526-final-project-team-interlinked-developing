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
            StartCoroutine(Knockback());
            StartCoroutine(CoolDown());
        }
    }

    private IEnumerator Knockback()
    {
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            PlayerBase pb = SingletonMaster.Instance.PlayerBase;
            Vector2 playerPos = pb.transform.position;
            
            pb.PlayKnockbackParticles();

            SingletonMaster.Instance.AudioManager.PlayPlayerSFX("PlayerKnockback"); // Free's sound

            RaycastHit2D[] hits = Physics2D.CircleCastAll(playerPos, m_knockBackRadius, Vector2.zero, 0.0f, m_knockBackMask);
            foreach (var hit in hits)
            {
                if (!pb.m_linkedObjects.Contains(hit.rigidbody.gameObject))
                {
                    hit.rigidbody.AddExplosionForce(m_knockBackStrength, playerPos, m_knockBackRadius);
                }
            }

            yield return new WaitForSeconds(m_ability.m_activeDuration);
            
            SingletonMaster.Instance.AbilityManager.AbilityFinished.Invoke(m_abilityType);
        }
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(m_ability.m_coolDown);
        m_canActivate = true;
    }
}
