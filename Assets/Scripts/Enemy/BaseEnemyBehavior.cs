using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class BaseEnemyBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    public float m_acceleration = 10.0f;
    public float m_maxSpeed = 50.0f;
    public List<Vector2> m_pathfindDirections = new List<Vector2>();
    public LayerMask m_pathfindIgnoreMasks;
    
    [Header("Visual Settings")] 
    public GameObject m_face;
    public float m_faceMoveFactor = 0.25f;
    
    public UnityEvent EnemyDamagedEvent { private set; get; } = new UnityEvent();
    
    private SpriteRenderer m_spriteRenderer;
    private Rigidbody2D m_RB;

    private Color m_orgColor;
    private Vector3 m_orgScale;
    
    private Vector2 m_randomDestinationDisp;
    
    /// <summary>
    /// Overwrite this for custom start behavior
    /// </summary>
    protected virtual void OnStart() { }
    
    /// <summary>
    /// Overwrite this for custom update behavior
    /// </summary>
    protected virtual void OnUpdate() { }

    private void Start()
    {
        OnStart();

        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_RB = GetComponent<Rigidbody2D>();

        m_orgColor = m_spriteRenderer.color;
        m_orgScale = transform.localScale;
        
        EnemyDamagedEvent.AddListener(OnDamaged);

        m_randomDestinationDisp = Random.insideUnitCircle.normalized;
    }

    private void OnDisable()
    {
        EnemyDamagedEvent.RemoveListener(OnDamaged);
    }

    private void FixedUpdate()
    {
        Vector3 actualplayerPos = SingletonMaster.Instance.PlayerController.transform.position;
        Vector3 dispPlayerPos = actualplayerPos + new Vector3(m_randomDestinationDisp.x, m_randomDestinationDisp.y, 0.0f) * 3.0f;
        
        Debug.DrawLine(transform.position, dispPlayerPos, Color.magenta);
        
        Vector3 toPlayerVec3 = (dispPlayerPos - transform.position).normalized;
        Vector3 toActualPlayer = (actualplayerPos - transform.position).normalized;
        if (Vector3.Distance(SingletonMaster.Instance.PlayerController.transform.position, transform.position) < 3.5f)
        {
            toPlayerVec3 = (actualplayerPos - transform.position).normalized;
        }
        Vector2 toPlayerVec2 = new Vector2(toPlayerVec3.x, toPlayerVec3.y);
        
        m_face.transform.localPosition = toActualPlayer * m_faceMoveFactor;

        Vector2 moveDir = toPlayerVec2;
        Vector2 worstDirection = Vector2.zero;
        float maxDotVal = -2.0f;
        foreach (var rayDirection in m_pathfindDirections)
        {
            var normRay = rayDirection.normalized;
            float dotVal = Vector2.Dot(normRay, toPlayerVec2);

            if (dotVal > maxDotVal)
            {
                maxDotVal = dotVal;
                worstDirection = rayDirection;
            }
        }
        
        RaycastHit2D obstacleHit =
            Physics2D.Raycast(transform.position, worstDirection, 2.5f, ~m_pathfindIgnoreMasks);
        if (obstacleHit)
        {
            Debug.Log("Detected obstacle");
            Debug.DrawLine(transform.position, 
                transform.position + new Vector3(worstDirection.x, worstDirection.y, 0.0f) * 2.5f, Color.cyan);
            
            Vector2 disp = new Vector2(-worstDirection.y, worstDirection.x);
            moveDir = disp;
        }
        
        if (m_RB.velocity.magnitude < m_maxSpeed)
        {
            m_RB.velocity += moveDir * m_acceleration * Time.fixedDeltaTime;
        }
    }

    private void Update()
    {
        OnUpdate();
    }

    protected virtual void OnDamaged()
    {
        // Juice Tweens
        m_spriteRenderer.DOKill(true);
        transform.DOKill(true);
        
        m_spriteRenderer.DOColor(Color.white, 0.1f).SetLoops(1, LoopType.Yoyo).SetEase(Ease.InOutFlash).OnComplete(() =>
        {
            m_spriteRenderer.color = m_orgColor;
        });
        transform.DOPunchScale(transform.localScale * 0.5f, 0.1f).OnComplete(() =>
        {
            transform.localScale = m_orgScale;
        });
        
        Debug.Log(gameObject + " enemy damaged");
    }
}
