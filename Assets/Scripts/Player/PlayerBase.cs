using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerBase : MonoBehaviour
{
    [Header("Basic Settings")] 
    public GameObject m_playerEntity;
    public HealthComponent m_healthComponent;
    public RopeComponent m_ropeComponent;
    public PlayerInput m_input;
    public float m_acceleration = 50.0f;
    public float m_maxSpeed = 100.0f;
    public Vector2 m_moveDirection;
    public Vector2 m_lastMoveDirection { private set; get; }
    
    [Header("Rope Settings")]
    public GameObject m_rope;
    public float m_clickRadius = 1.0f;
    public float m_connectRadius = 10.0f;
    public int m_maxRopeConnections = 5;
    public int m_curRopeConnections = 0;
    public float m_throwStrength = 50.0f;
    public float m_throwAutoTargetRadius = 10.0f;
    public float m_throwAutoTargetRange = 10.0f;
    public LayerMask m_throwTargetMask;
    public GameObject m_linkObjectsParent;
    public LayerMask m_connectableLayers;
    public List<GameObject> m_linkedObjects = new List<GameObject>();
    private List<Vector3> m_linkedDisplacements = new List<Vector3>();
    private List<Vector3> m_ropeDisplacements = new List<Vector3>();
    

    [Header("Visual Settings")] 
    [SerializeField] private CinemachineVirtualCamera m_cinemachine;
    [SerializeField] private float m_cameraZoomFactor = 0.025f;
    [SerializeField] private GameObject m_face;
    [SerializeField] private float m_faceMoveFactor = 0.25f;
    public bool m_isFollowCam = true;

    [Header("Ability")] 
    public bool m_isDashing = false;

    private Rigidbody2D m_RB;
    private SpriteRenderer m_spriteRenderer;
    private Color m_orgColor;
    private Vector3 m_orgScale;

    private bool m_isMouseDown;
    
    private Vector2 m_drawpos;
    private float m_orgZoom;

    private bool m_isInitiated = false;

    private GameObject m_bestRopeConnectTarget = null;
    private GameObject m_bestRopeDisconnectTarget = null;

    private void Start()
    {
        // Adding self to linked object list first
        m_linkedObjects.Add(gameObject);
        m_orgZoom = m_cinemachine.m_Lens.OrthographicSize;
        m_RB = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_orgColor = m_spriteRenderer.color;
        m_orgScale = transform.localScale;

        m_healthComponent.m_isLinked = true;

        if (m_isFollowCam)
        {
            m_cinemachine.m_Follow = transform;
        }
        else
        {
            m_cinemachine.m_Lens.OrthographicSize = 20.0f;
        }
        
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinkedItem);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinkedItem);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnUnlinkedItem(GameObject obj, GameObject instigator)
    {
        if (m_linkedObjects.Contains(obj) && instigator.CompareTag("Player"))
        {
            m_linkedObjects.Remove(obj);
        }
    }

    private void OnLinkedItem(GameObject obj, GameObject instigator)
    {
        if (!m_linkedObjects.Contains(obj) && instigator.CompareTag("Player"))
        {
            m_linkedObjects.Add(obj);
        }
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        if (this != null)
        {
            m_linkedDisplacements.Clear();
            m_ropeDisplacements.Clear();

            foreach (var obj in m_linkedObjects)
            {
                Vector3 disp = obj.transform.position - transform.position;
                m_linkedDisplacements.Add(disp);
                obj.GetComponent<Rigidbody2D>().isKinematic = true;
                // Debug.Log(disp);
            }

            for (int i = 0; i < m_rope.transform.childCount; i++)
            {
                Rigidbody2D rb = m_rope.transform.GetChild(i).gameObject.GetComponent<Rigidbody2D>();
                rb.isKinematic = true;
                Vector3 disp = m_rope.transform.GetChild(i).position - transform.position;
                m_ropeDisplacements.Add(disp);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (this != null)
        {
            transform.position = Vector3.zero;
            StartCoroutine(InitCoroutine());
        }
    }

    private IEnumerator InitCoroutine()
    {
        // yield return new WaitForSeconds(0.5f);
        yield return null;
        for (int i = 0; i < m_linkedObjects.Count; i++)
        {
            // Debug.Log(m_linkedDisplacements[i]);
            m_linkedObjects[i].transform.position = transform.position + m_linkedDisplacements[i];
            m_linkedObjects[i].GetComponent<Rigidbody2D>().isKinematic = false;
        }
        
        for (int i = 0; i < m_rope.transform.childCount; i++)
        {
            Rigidbody2D rb = m_rope.transform.GetChild(i).gameObject.GetComponent<Rigidbody2D>();
            m_rope.transform.GetChild(i).position = transform.position + m_ropeDisplacements[i];
            rb.isKinematic = false;
        }
    }

    /// <summary>
    /// For physics based movements
    /// </summary>
    private void FixedUpdate()
    {
        float hori = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        
        Vector3 faceDir = new Vector3(hori, vert, 0.0f) * m_faceMoveFactor;
        m_face.transform.localPosition = faceDir;

        if (!m_isDashing)
        {
            m_RB.velocity += m_moveDirection * m_acceleration * Time.fixedDeltaTime;
            if (m_RB.velocity.magnitude > m_maxSpeed)
            {
                m_RB.velocity = m_moveDirection * m_maxSpeed;
            }
        }
    }

    /// <summary>
    /// Debug draw stuff - pickup range etc...
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(m_drawpos, m_connectRadius);
    }

    void Update()
    {
        CameraZoomControl();
        CheckBestRopeTargetToConnect();
        CheckBestRopeTargetToDisconnect();
        HighlightConnections();
    }
    
    private void CheckBestRopeTargetToConnect()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
        
        RaycastHit2D[] results = new RaycastHit2D[20];
        int num = Physics2D.CircleCastNonAlloc(mouseWorldPos, m_clickRadius, Vector2.zero,
            results, 0.0f, m_connectableLayers);

        float minDist = float.MaxValue;

        GameObject curBestTarget = null;
        for (int i = 0; i < num; i++)
        {
            float distToPlayer = (results[i].transform.position - transform.position).magnitude;
            if (distToPlayer <= m_connectRadius && m_linkedObjects.Count <= m_maxRopeConnections)
            {
                if (!m_linkedObjects.Contains(results[i].rigidbody.gameObject))
                {
                    float dist = Vector3.Distance(results[i].transform.position, mouseWorldPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        curBestTarget = results[i].rigidbody.gameObject;
                    }
                }
            }
        }

        // Setting the best target & doing highlight visuals
        if (curBestTarget != null && curBestTarget != m_bestRopeConnectTarget)
        {
            if (m_bestRopeConnectTarget != null)
            {
                m_bestRopeConnectTarget.GetComponent<RopeComponent>().HideHighlight();
            }

            m_bestRopeConnectTarget = curBestTarget;
            m_bestRopeConnectTarget.GetComponent<RopeComponent>().ShowHighlight(true);
        }
        else if (curBestTarget == null && m_bestRopeConnectTarget != null)
        {
            m_bestRopeConnectTarget.GetComponent<RopeComponent>().HideHighlight();
            m_bestRopeConnectTarget = null;
        }
    }
    
    private void CheckBestRopeTargetToDisconnect()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
        
        RaycastHit2D[] results = new RaycastHit2D[20];
        int num = Physics2D.CircleCastNonAlloc(mouseWorldPos, m_clickRadius, Vector2.zero,
            results, 0.0f, m_connectableLayers);

        float minDist = float.MaxValue;
        GameObject curBestTarget = null;
        for (int i = 0; i < num; i++)
        {
            if (m_linkedObjects.Contains(results[i].rigidbody.gameObject))
            {
                float dist = Vector3.Distance(results[i].transform.position, mouseWorldPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    curBestTarget = results[i].rigidbody.gameObject;
                }
            }
        }
                
        if (curBestTarget != null && curBestTarget != m_bestRopeDisconnectTarget)
        {
            if (m_bestRopeDisconnectTarget != null)
            {
                m_bestRopeDisconnectTarget.GetComponent<RopeComponent>().HideHighlight();
            }

            m_bestRopeDisconnectTarget = curBestTarget;
            m_bestRopeDisconnectTarget.GetComponent<RopeComponent>().ShowHighlight(false);
        }
        else if (curBestTarget == null && m_bestRopeDisconnectTarget != null)
        {
            m_bestRopeDisconnectTarget.GetComponent<RopeComponent>().HideHighlight();
            m_bestRopeDisconnectTarget = null;
        }
    }
    
    private void HighlightConnections()
    {
        if (m_bestRopeConnectTarget != null && m_bestRopeDisconnectTarget == null)
        {
            m_bestRopeConnectTarget.GetComponent<RopeComponent>().ShowHighlight(true);
        }
        else if (m_bestRopeConnectTarget == null && m_bestRopeDisconnectTarget != null)
        {
            m_bestRopeDisconnectTarget.GetComponent<RopeComponent>().ShowHighlight(false);
        }
    }

    private void CameraZoomControl()
    {
        if (m_isFollowCam)
        {
            // This can have some real issue with floats
            Vector2 minCorner = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxCorner = new Vector2(float.MinValue, float.MinValue);

            foreach (var link in m_linkedObjects)
            {
                if (link != null)
                {
                    Vector2 pos = Camera.main.WorldToScreenPoint(link.transform.position);

                    maxCorner = Vector2.Max(maxCorner, pos);
                    minCorner = Vector2.Min(minCorner, pos);
                }
            }

            m_cinemachine.m_Lens.OrthographicSize =
                Vector2.Distance(minCorner, maxCorner) * m_cameraZoomFactor + m_orgZoom;
        }
    }
    
    public void Move(InputAction.CallbackContext context)
    {
        m_moveDirection = context.ReadValue<Vector2>();

        if (m_moveDirection.magnitude > 0.0f)
        {
            m_lastMoveDirection = m_moveDirection;
        }
    }

    public void RopeConnect(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (m_bestRopeConnectTarget != null)
            {
                // TODO: We need to change this...
                int level = SceneManager.GetActiveScene().buildIndex;
                MetricsManager.Instance.m_metricsData.RecordRopeOperations(level, true);
                RequestRopeConnect(m_bestRopeConnectTarget.GetComponent<Rigidbody2D>());
            }
        }
    }

    public void RopeDisconnect(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (m_bestRopeDisconnectTarget != null)
            {
                // TODO: We need to change this...
                int level = SceneManager.GetActiveScene().buildIndex;
                MetricsManager.Instance.m_metricsData.RecordRopeOperations(level, false);
                
                // Honing in to the nearest enemy / dangerous object
                RemoveLinkedObject(m_bestRopeDisconnectTarget);
            }
        }
    }
    
    // For connecting ropes
    private void RequestRopeConnect(Rigidbody2D hitBody)
    {
        Debug.Log("Requesting Connect to: " + hitBody.gameObject);
        GameObject hitObject = hitBody.gameObject;
        m_drawpos = hitBody.position;
        
        // Do a distance check - For Player
        // float dist = (hitBody.transform.position - transform.position).magnitude;
        // if (dist <= m_connectRadius && m_linkedObjects.Count <= m_maxRopeConnections)
        {
            Debug.Log("Connected " + hitObject + " to player"); 
            m_ropeComponent.GenerateRope(hitObject);
        }
        m_bestRopeConnectTarget.GetComponent<RopeComponent>().HideHighlight();
        m_bestRopeConnectTarget = null;
    }

    // For removing linked objects
    public void RemoveLinkedObject(GameObject obj)
    {
        RopeComponent targetRope = obj.GetComponent<RopeComponent>();
        if (targetRope != null)
        {
            targetRope.DetachRope(gameObject);
            targetRope.HideHighlight();
            
            AutomaticThrowToNearestTarget(obj);
            m_bestRopeDisconnectTarget = null;
        }
    }

    private void AutomaticThrowToNearestTarget(GameObject obj)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        RaycastHit2D[] results = new RaycastHit2D[20];
        Vector2 velDir = rb.velocity;
        Debug.Log(velDir);
        int hits = Physics2D.CircleCastNonAlloc(rb.position, m_throwAutoTargetRadius, velDir, results, m_throwAutoTargetRange, m_throwTargetMask);
        
        float minDist = float.MaxValue;
        GameObject curBestTarget = null;
        for (int i = 0; i < hits; i++)
        {
            if (results[i].collider.gameObject != obj)
            {
                Vector2 toTarget = results[i].transform.position - transform.position;
                float dist = Vector3.Distance(results[i].transform.position, rb.position);
                
                if (dist < minDist && Vector2.Dot(velDir, toTarget) > 0.6f)
                {
                    minDist = dist;
                    curBestTarget = results[i].collider.gameObject;
                }
            }
        }

        if (curBestTarget != null)
        {
            Vector2 dir = (curBestTarget.transform.position - obj.transform.position).normalized;
            rb.AddForce(dir.normalized * m_throwStrength, ForceMode2D.Impulse);
            Vector3 drawDir = dir.normalized;
            Debug.DrawLine(obj.transform.position, obj.transform.position + drawDir * 10.0f, Color.green, 10.0f);
        }
        else
        {
            Debug.Log("No Target Found...");
        }
    }
    
    // Ability Stuff
    public void ActivateAbility(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("Dash");
            SingletonMaster.Instance.AbilityManager.ActivateAbility.Invoke(AbilityManager.AbilityTypes.Dash);
        }
    }
    
}
