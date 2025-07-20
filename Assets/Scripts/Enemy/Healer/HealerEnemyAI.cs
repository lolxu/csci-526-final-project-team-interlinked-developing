using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HealerEnemyAI : BaseEnemyAI
{
    private enum HealerState
    {
        MovingToHeal,
        Healing,
        MovingToPlayer,
        Idle
    }

    [Header("Healer Settings")] 
    [SerializeField] private float m_healRate = 0.5f;
    [SerializeField] private float m_healTimeout = 1.0f;
    [SerializeField] private HealerState m_healerState;
    
    private RopeComponent m_ropeComponent;
    private Coroutine m_healTimeoutCoroutine;
    
    protected override void Start()
    {
        base.Start();
        
        SingletonMaster.Instance.EventManager.EnemyDamagedEvent.AddListener(OnEnemyDamaged);
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.AddListener(OnEnemyDeath);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
        SingletonMaster.Instance.EventManager.StealSuccessEvent.AddListener(OnStealSuccess);

        m_healerState = HealerState.MovingToPlayer;
        m_ropeComponent = GetComponent<RopeComponent>();
        
        FindingHealTarget();
    }

    private void OnStealSuccess(GameObject item, GameObject enemy)
    {
        // We are def healing at this point
        if (item == m_moveTarget && enemy == gameObject)
        {
            // Resetting healer
            HealthComponent hc = m_moveTarget.GetComponent<HealthComponent>();
            hc.m_isHealing = false;
            hc.m_healer = null;
            
            m_moveTarget = SingletonMaster.Instance.PlayerBase.gameObject;
            m_healerState = HealerState.MovingToPlayer;
            m_healTimeoutCoroutine = StartCoroutine(HealTimeout());
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (m_healTimeoutCoroutine != null)
        {
            StopCoroutine(m_healTimeoutCoroutine);
        }

        SingletonMaster.Instance.EventManager.EnemyDamagedEvent.RemoveListener(OnEnemyDamaged);
        SingletonMaster.Instance.EventManager.EnemyDeathEvent.RemoveListener(OnEnemyDeath);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
        SingletonMaster.Instance.EventManager.StealSuccessEvent.RemoveListener(OnStealSuccess);
    }
    
    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (m_moveTarget == obj && gameObject == instigator)
        {
            HealthComponent hc = m_moveTarget.GetComponent<HealthComponent>();
            if (hc != null)
            {
                hc.m_healer = null;
                hc.m_isHealing = false;
            }
            
            m_healerState = HealerState.MovingToPlayer;
            m_moveTarget = null;
            m_healTimeoutCoroutine = StartCoroutine(HealTimeout());
        }
    }

    private IEnumerator HealTimeout()
    {
        yield return new WaitForSeconds(m_healTimeout);
        FindingHealTarget();
    }

    private void OnEnemyDamaged(GameObject enemy)
    {
        if (enemy != gameObject)
        {
            if (m_healerState == HealerState.MovingToPlayer)
            {
                FindingHealTarget();
            }
        }
    }
    
    private void OnEnemyDeath(GameObject enemy)
    {
        if (enemy != gameObject)
        {
            if (enemy == m_moveTarget)
            {
                m_healerState = HealerState.MovingToPlayer;
                m_moveTarget = null;
                FindingHealTarget();
            }
        }
        else
        {
            if (m_moveTarget != null)
            {
                HealthComponent hc = m_moveTarget.GetComponent<HealthComponent>();
                if (hc != null)
                {
                    hc.m_healer = null;
                    hc.m_isHealing = false;
                }
            }
        }
    }

    protected override void FixedUpdate()
    {
        CheckingStates();

        if (m_moveTarget != null)
        {
            Vector3 targetPos = m_moveTarget.transform.position;
            Vector3 faceDir = (targetPos - transform.position).normalized;

            switch (m_healerState)
            {
                case HealerState.MovingToHeal:
                {
                    MoveBehavior();
                    break;
                }
                case HealerState.Healing:
                {
                    MoveBehavior();
                    HealingEnemy();
                    break;
                }
                case HealerState.MovingToPlayer:
                {
                    MoveBehavior();
                    FindingHealTarget();
                    break;
                }
                case HealerState.Idle:
                {
                    break;
                }
            }

            // moving face
            m_face.transform.localPosition = faceDir * m_faceMoveFactor;
        }
    }
    
    protected override Vector3 GetTargetPosition()
    {
        Vector3 myTargetPos = m_moveTarget.transform.position + new Vector3(m_randomDestinationDisp.x, m_randomDestinationDisp.y, 0.0f);;
        
        return myTargetPos;
    }

    private void CheckingStates()
    {
        if (m_moveTarget != null)
        {
            // Debug.Log("Healer trying to go to damaged enemies...");

            if (!m_moveTarget.CompareTag("Player"))
            {
                if (m_healerState != HealerState.Healing)
                {
                    float dist = Vector3.Distance(transform.position, m_moveTarget.transform.position);
                    if (dist < m_surroundDistance)
                    {
                        Debug.Log("Healing: " + m_moveTarget.transform.parent.gameObject);
                        m_healerState = HealerState.Healing;
                        m_ropeComponent.GenerateRope(m_moveTarget);
                    }
                    else
                    {
                        m_healerState = HealerState.MovingToHeal;
                    }
                }
                
            }
        }
        else
        {
            if (SingletonMaster.Instance.PlayerBase != null)
            {
                m_healerState = HealerState.MovingToPlayer;
                m_moveTarget = SingletonMaster.Instance.PlayerBase.gameObject;
            }
            else
            {
                m_healerState = HealerState.Idle;
            }
        }
    }

    private void FindingHealTarget()
    {
        if (m_moveTarget == null || m_moveTarget.CompareTag("Player"))
        {
            // Getting list of damaged enemies & finding the closest one
            float minDist = float.MaxValue;
            GameObject newTarget = null;
            foreach (var enemy in SingletonMaster.Instance.WaveManager.m_enemies)
            {
                GameObject enemyObj = enemy.transform.GetChild(0).gameObject;
                if (enemyObj != gameObject)
                {
                    HealthComponent hc = enemyObj.GetComponent<HealthComponent>();

                    // Making sure only one healer connects to one enemy
                    if (hc != null && hc.m_healer == null && hc.m_health < hc.m_maxHealth)
                    {
                        float dist = Vector3.Distance(transform.position, enemyObj.transform.position);
                        if (dist < minDist)
                        {
                            hc.m_healer = gameObject;
                            minDist = dist;
                            newTarget = enemyObj;
                        }
                    }
                }
            }

            if (newTarget != null)
            {
                m_moveTarget = newTarget;
                m_healerState = HealerState.MovingToHeal;
            }
            else
            {
                if (SingletonMaster.Instance.PlayerBase != null)
                {
                    m_moveTarget = SingletonMaster.Instance.PlayerBase.gameObject;
                    m_healerState = HealerState.MovingToPlayer;
                }
                else
                {
                    m_healerState = HealerState.Idle;
                }
            }
        }
    }

    private void HealingEnemy()
    {
        if (m_moveTarget != null)
        {
            HealthComponent hc = m_moveTarget.GetComponent<HealthComponent>();
            if (hc != null)
            {
                if (hc.m_health < hc.m_maxHealth)
                {
                    hc.m_health += m_healRate * Time.deltaTime;
                    hc.m_isHealing = true;
                }
                else
                {
                    hc.m_isHealing = false;
                    hc.m_health = hc.m_maxHealth;
                    hc.m_healer = null;
                    
                    // Disconnect
                    m_moveTarget.GetComponent<RopeComponent>().DetachEnemy(gameObject);
                    m_moveTarget = null;
                }
            }
        }
    }
}
