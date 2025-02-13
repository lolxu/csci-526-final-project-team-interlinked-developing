using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BaseEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")] 
    public float m_acceleration = 10.0f;
    public float m_maxSpeed = 50.0f;
    public int m_numDirections = 12;
    public float m_raycastDistance = 5.0f;
    public LayerMask m_pathfindIgnoreMasks;
    
    [Header("Visual Settings")]
    [SerializeField] protected GameObject m_face;
    [SerializeField] protected float m_faceMoveFactor = 0.25f;

    private List<Vector2> m_pathfindDirections = new List<Vector2>();
    private Rigidbody2D m_RB;
    private Vector2 m_randomDestinationDisp;
    private Vector2 m_moveDirection = Vector2.zero;
    private bool m_overrideMovement = false;

    private void Start()
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
        
        m_randomDestinationDisp = Random.insideUnitCircle.normalized * 5.0f;
        
        SingletonMaster.Instance.EventManager.StealStartedEvent.AddListener(OnStealStarted);
        SingletonMaster.Instance.EventManager.StealEndedEvent.AddListener(OnStealEnded);
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.StealStartedEvent.RemoveListener(OnStealStarted);
        SingletonMaster.Instance.EventManager.StealEndedEvent.RemoveListener(OnStealEnded);
    }

    private void OnStealEnded(GameObject item, GameObject enemy)
    {
        if (enemy == gameObject)
        {
            m_overrideMovement = false;
        }
    }

    private void OnStealStarted(GameObject item, GameObject enemy)
    {
        if (enemy == gameObject)
        {
            m_overrideMovement = true;
        }
    }

    private void FixedUpdate()
    {
        // Use a value based algorithm to pick directions
        // Raycast to all directions and check for obstacles
        // The value associated with the direction will be values of dot product
        // Highest it is meaning the more close to the player it is.

        // Getting player directions with some randomization
        if (SingletonMaster.Instance.PlayerBase != null)
        {
            Vector3 playerPos = SingletonMaster.Instance.PlayerBase.transform.position;

            Vector3 faceDir = Vector3.zero;
            if (!m_overrideMovement)
            {
                MoveToPlayer();
                faceDir = (playerPos - transform.position).normalized;
            }
            else
            {
                Debug.Log("Tug of war");
                Vector3 playerDir = SingletonMaster.Instance.PlayerBase.m_moveDirection;
                m_moveDirection = -playerDir;
                faceDir = m_moveDirection;
                
                // Moving using the best direction
                m_RB.velocity += m_moveDirection * m_acceleration * 10.0f * Time.fixedDeltaTime;
            }
            
            m_face.transform.localPosition = faceDir * m_faceMoveFactor;
        }
    }

    private void MoveToPlayer()
    {
        Vector3 playerPos = SingletonMaster.Instance.PlayerBase.transform.position;
        Vector3 targetPos = Vector2.zero;
        if (Vector3.Distance(playerPos, transform.position) > 5.5f)
        {
            targetPos = playerPos + new Vector3(m_randomDestinationDisp.x, m_randomDestinationDisp.y, 0.0f);
        }
        else
        {
            targetPos = playerPos;
        }

        Vector3 toPlayerVec3 = targetPos - transform.position;
        Vector2 toPlayer = toPlayerVec3;
        toPlayer = toPlayer.normalized;

        Debug.DrawLine(transform.position, transform.position + new Vector3(toPlayer.x, toPlayer.y, 0.0f) * 10.0f,
            Color.red);

        // Calculating direction values
        float bestDotVal = -2.0f;
        Vector2 bestDirection = Vector2.zero;
        foreach (var direction in m_pathfindDirections)
        {
            Debug.DrawLine(transform.position,
                transform.position + new Vector3(direction.x, direction.y, 0.0f) * m_raycastDistance, Color.cyan);

            float dotVal = Vector2.Dot(direction, toPlayer);

            // Raycast to check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, m_raycastDistance,
                ~m_pathfindIgnoreMasks);
            if (hit)
            {
                dotVal = -1.0f;
            }

            // Checking dot values
            if (dotVal > bestDotVal)
            {
                bestDotVal = dotVal;
                bestDirection = direction;
            }
        }

        if (bestDotVal > 0.15f)
        {
            m_moveDirection = bestDirection;
        }
        else
        {
            m_moveDirection = Vector2.zero;
        }

        Debug.DrawLine(transform.position,
            transform.position + new Vector3(bestDirection.x, bestDirection.y, 0.0f) * m_raycastDistance,
            Color.magenta);

        // Moving using the best direction
        if (m_RB.velocity.magnitude < m_maxSpeed)
        {
            m_RB.velocity += m_moveDirection * m_acceleration * Time.fixedDeltaTime;
        }
    }
}
