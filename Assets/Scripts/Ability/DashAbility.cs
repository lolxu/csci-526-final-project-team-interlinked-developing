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
    [SerializeField] private float m_dashMult = 100.0f;

    private Color m_orgColor;

    private bool m_canActivate = true;
    
    private void Start()
    {
        SingletonMaster.Instance.AbilityManager.ActivateAbility.AddListener(OnAbilityActivated);
        
        // Get player color
        m_orgColor = SingletonMaster.Instance.PlayerBase.gameObject.GetComponent<SpriteRenderer>().color;
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
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            PlayerBase pb = SingletonMaster.Instance.PlayerBase;
            GameObject player = pb.gameObject;
            Vector2 dashDir = pb.m_moveDirection;

            player.GetComponent<Collider2D>().excludeLayers = m_dashMasks;
            SingletonMaster.Instance.PlayerBase.m_isDashing = true;
            Color newColor = m_orgColor;
            newColor.a = 0.25f;
            player.GetComponent<SpriteRenderer>().color = newColor;

            // Also disabling collision on all connected stuff
            // TODO: Toggle this for testing
            if (false)
            {
                int numLinks = pb.m_rope.transform.childCount;
                for (int i = 0; i < numLinks; i++)
                {
                    GameObject curLink = pb.m_rope.transform.GetChild(i).gameObject;
                    curLink.GetComponent<Collider2D>().excludeLayers = m_dashMasks;
                }
            }

            float time = 0.0f;
            while (time < m_ability.m_activeDuration)
            {
                if (SingletonMaster.Instance.PlayerBase != null)
                {
                    player.GetComponent<Rigidbody2D>().velocity = dashDir * m_dashMult;
                    time += Time.deltaTime;
                    yield return null;
                }
                else
                {
                    break;
                }
            }

            if (SingletonMaster.Instance.PlayerBase != null)
            {
                player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                SingletonMaster.Instance.PlayerBase.m_isDashing = false;
                player.GetComponent<Collider2D>().excludeLayers = default;
                player.GetComponent<SpriteRenderer>().color = m_orgColor;
            }
        }
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(m_ability.m_coolDown);
        m_canActivate = true;
    }
}
