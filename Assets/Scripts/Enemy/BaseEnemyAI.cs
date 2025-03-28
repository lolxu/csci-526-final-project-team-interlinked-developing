using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BaseEnemyAI : MonoBehaviour
{
    // A simple state machine...
    public enum EnemyAIState
    {
        Moving,
        TugOfWar,
        Attack,
        Idle
    }
    public EnemyAIState m_state = EnemyAIState.Idle;
    
    [Header("Movement Settings")] 
    public float m_acceleration = 10.0f;
    public float m_maxSpeed = 6.0f;
    public int m_numDirections = 12;
    public float m_raycastDistance = 5.0f;
    public float m_surroundDistance = 6.0f;
    public LayerMask m_pathfindIgnoreMasks;
    public Vector2 m_moveDirection { private set; get; } = Vector2.zero;
    [SerializeField] protected float m_invisibleThreshold = 5.0f;
    [SerializeField] protected GameObject m_moveTarget;
    
    [Header("Visual Settings")]
    [SerializeField] protected GameObject m_face;
    [SerializeField] protected float m_faceMoveFactor = 0.25f;
    
    protected bool m_overrideMovement = false;

    protected List<Vector2> m_pathfindDirections = new List<Vector2>();
    protected Rigidbody2D m_RB;
    protected Vector2 m_randomDestinationDisp;

    protected virtual void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
        m_randomDestinationDisp = Random.insideUnitCircle.normalized;

        float angleDisp = 2 * Mathf.PI / m_numDirections;
        
        m_pathfindDirections.Add(Vector2.up);
        for (int i = 1; i < m_numDirections; i++)
        {
            Vector2 lastDir = m_pathfindDirections[i - 1];
            float x = Mathf.Cos(angleDisp) * lastDir.x - Mathf.Sin(angleDisp) * lastDir.y;
            float y = Mathf.Sin(angleDisp) * lastDir.x + Mathf.Cos(angleDisp) * lastDir.y;
            Vector2 newDir = new Vector2(x, y);
            newDir = newDir.normalized;
            m_pathfindDirections.Add(newDir);
        }
        
        m_randomDestinationDisp = Random.insideUnitCircle.normalized * m_surroundDistance;
        
        // Setting state
        m_state = EnemyAIState.Moving;
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            m_moveTarget = SingletonMaster.Instance.PlayerBase.gameObject;
        }
        
        SingletonMaster.Instance.EventManager.StealStartedEvent.AddListener(OnStealStarted);
        SingletonMaster.Instance.EventManager.StealEndedEvent.AddListener(OnStealEnded);
    }

    protected virtual void OnDisable()
    {
        SingletonMaster.Instance.EventManager.StealStartedEvent.RemoveListener(OnStealStarted);
        SingletonMaster.Instance.EventManager.StealEndedEvent.RemoveListener(OnStealEnded);
    }

    private void OnStealEnded(GameObject item, GameObject enemy)
    {
        if (enemy == gameObject)
        {
            m_state = EnemyAIState.Moving;
        }
    }

    private void OnStealStarted(GameObject item, GameObject enemy)
    {
        if (enemy == gameObject)
        {
            m_state = EnemyAIState.TugOfWar;
        }
    }

    protected virtual void FixedUpdate()
    {
        // Use a value based algorithm to pick directions
        // Raycast to all directions and check for obstacles
        // The value associated with the direction will be values of dot product
        // Highest it is meaning the more close to the player it is.

        // Getting player directions with some randomization
        if (SingletonMaster.Instance.PlayerBase != null && m_moveTarget != null)
        {
            Vector3 targetPos = m_moveTarget.transform.position;
            Vector3 faceDir = (targetPos - transform.position).normalized;

            if (!m_overrideMovement)
            {
                // Debug.Log(m_state);
                switch (m_state)
                {
                    case EnemyAIState.Moving:
                    {
                        MoveBehavior();
                        break;
                    }
                    case EnemyAIState.TugOfWar:
                    {
                        // Moving against player movement - TUG OF WAR mechanic
                        TugOfWarBehavior();
                        faceDir = m_moveDirection;
                        break;
                    }
                    case EnemyAIState.Attack:
                    {
                        AttackBehavior();
                        break;
                    }
                    case EnemyAIState.Idle:
                    {
                        IdleBehavior();
                        break;
                    }
                }
            }

            // moving face
            m_face.transform.localPosition = faceDir * m_faceMoveFactor;
        }
        else
        {
            m_state = EnemyAIState.Idle;
        }
    }

    protected virtual void IdleBehavior()
    {
        
    }

    protected virtual void AttackBehavior()
    {
        
    }
    
    protected virtual void TugOfWarBehavior()
    {
        Vector3 playerDir = SingletonMaster.Instance.PlayerBase.m_moveDirection;
        m_moveDirection = -playerDir;
        m_RB.velocity += m_moveDirection * m_acceleration * 10.0f * Time.fixedDeltaTime;
    }

    protected virtual void MoveBehavior()
    {
        Vector3 toTargetVec3 = GetTargetPosition() - transform.position;
        Vector2 toTarget = toTargetVec3;
        toTarget = toTarget.normalized;

        Debug.DrawLine(transform.position, transform.position + new Vector3(toTarget.x, toTarget.y, 0.0f) * 10.0f,
            Color.red);
        
        // Calculating direction values
        float bestDotVal = -2.0f;
        Vector2 bestDirection = Vector2.zero;
        foreach (var direction in m_pathfindDirections)
        {
            float dotVal = Vector2.Dot(direction, toTarget);

            // Raycast to check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, m_raycastDistance, ~m_pathfindIgnoreMasks);
            if (hit)
            {
                dotVal = -1.0f;
                m_moveDirection -= direction;
            }
            else
            {
                Debug.DrawLine(transform.position,
                    transform.position + new Vector3(direction.x, direction.y, 0.0f) * m_raycastDistance, Color.cyan);
            }

            // Checking dot values
            if (dotVal > bestDotVal)
            {
                bestDotVal = dotVal;
                bestDirection = direction;
            }
        }
        
        m_moveDirection += bestDirection;
        m_moveDirection = m_moveDirection.normalized;

        // The actual move direction
        Debug.DrawLine(transform.position,
            transform.position + new Vector3(m_moveDirection.x, m_moveDirection.y, 0.0f) * 5.0f,
            Color.magenta);

        // Moving using the best direction
        if (m_RB.velocity.magnitude < m_maxSpeed)
        {
            m_RB.velocity += m_moveDirection * m_acceleration * Time.fixedDeltaTime;
        }
    }

    protected virtual Vector3 GetTargetPosition()
    {
        Vector3 myTargetPos = m_moveTarget.transform.position;
        Vector3 targetPos;
        if (Vector3.Distance(myTargetPos, transform.position) > m_surroundDistance)
        {
            targetPos = myTargetPos + new Vector3(m_randomDestinationDisp.x, m_randomDestinationDisp.y, 0.0f);
        }
        else
        {
            targetPos = myTargetPos;
        }

        return targetPos;
    }
}
