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
    public PlayerInput m_input;
    public float m_acceleration = 50.0f;
    public float m_maxSpeed = 100.0f;
    
    [Header("Rope Settings")]
    public GameObject m_rope;
    public float m_clickRadius = 1.0f;
    public float m_connectRadius = 10.0f;
    public GameObject m_linkObjectsParent;
    public List<GameObject> m_linkedObjects = new List<GameObject>();
    private List<Vector3> m_linkedDisplacements = new List<Vector3>();
    private List<Vector3> m_ropeDisplacements = new List<Vector3>();

    [Header("Visual Settings")] 
    [SerializeField] private CinemachineVirtualCamera m_cinemachine;
    [SerializeField] private float m_cameraZoomFactor = 0.025f;
    [SerializeField] private GameObject m_face;
    [SerializeField] private float m_faceMoveFactor = 0.25f;

    private Rigidbody2D m_RB;
    private SpriteRenderer m_spriteRenderer;
    private Color m_orgColor;
    private Vector3 m_orgScale;

    private bool m_isMouseDown;

    private Vector2 m_moveDirection;
    private Vector2 m_drawpos;
    private float m_orgZoom;

    private bool m_isInitiated = false;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
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
        
        if (m_RB.velocity.magnitude < m_maxSpeed)
        {
            m_RB.velocity += m_moveDirection * m_acceleration * Time.fixedDeltaTime;
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
    }

    private void CameraZoomControl()
    {
        // This can have some real issue with floats
        Vector2 minCorner = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxCorner = new Vector2(float.MinValue, float.MinValue);

        foreach (var link in m_linkedObjects)
        {
            Vector2 pos = Camera.main.WorldToScreenPoint(link.transform.position);
            
            maxCorner = Vector2.Max(maxCorner, pos);
            minCorner = Vector2.Min(minCorner, pos);
        }
        
        m_cinemachine.m_Lens.OrthographicSize = Vector2.Distance(minCorner, maxCorner) * m_cameraZoomFactor + m_orgZoom;
    }
    
    public void Move(InputAction.CallbackContext context)
    {
        m_moveDirection = context.ReadValue<Vector2>();
    }
    
    public void Fire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SingletonMaster.Instance.EventManager.StartFireEvent.Invoke();
        }
        else
        {
            SingletonMaster.Instance.EventManager.StopFireEvent.Invoke();
        }
    }

    public void RopeOperations(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);

            // Checking if mouse hits the unconnected hit boxes
            RaycastHit2D pickupHit = Physics2D.CircleCast(mouseWorldPos, m_clickRadius, Vector2.zero,
                0.0f, LayerMask.GetMask("Unconnected"));
            if (pickupHit)
            {
                Rigidbody2D hitBody = pickupHit.rigidbody;
                // Double checking in case weird shit adds this thing twice
                if (!m_linkedObjects.Contains(hitBody.gameObject))
                {
                    GameObject usingRope = hitBody.gameObject.GetComponent<RopeGenerator>().m_myRopePrefab;
                    
                    RequestRopeConnect(hitBody, usingRope);
                }
                return;
            }

            // Checking if mouse hits connected loot
            RaycastHit2D hit = Physics2D.CircleCast(mouseWorldPos, m_clickRadius, Vector2.zero,
                0.0f, LayerMask.GetMask("Player"));

            if (hit && hit.rigidbody != m_RB)
            {
                RemoveLinkedObject(hit.rigidbody.gameObject);
            }
        }
    }
    
    // For connecting ropes
    private void RequestRopeConnect(Rigidbody2D hitBody, GameObject usingRope)
    {
        GameObject hitObject = hitBody.gameObject;
        m_drawpos = hitBody.position;
        
        // Do a circle cast
        RaycastHit2D[] results = new RaycastHit2D[100];
        int numHit = Physics2D.CircleCastNonAlloc(hitBody.position, m_connectRadius, 
            Vector2.zero, results, m_connectRadius, LayerMask.GetMask("Player"));
        // Debug.Log(numHit);
        
        float minDist = Single.MaxValue;
        GameObject bestConnector = null;
        for (int i = 0; i < numHit; i++)
        {
            // if (results[i].transform.gameObject != gameObject)
            RopeGenerator rp = results[i].rigidbody.gameObject.GetComponent<RopeGenerator>();
            if (rp != null)
            {
                float dist = Vector2.Distance(results[i].rigidbody.gameObject.transform.position, hitBody.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestConnector = results[i].rigidbody.gameObject;
                }
            }
        }
        
        if (bestConnector != null)
        {
            Debug.Log("Connected " + hitObject + " to " + bestConnector); 
            bestConnector.GetComponent<RopeGenerator>().m_next.Add(hitObject);
            bestConnector.GetComponent<RopeGenerator>().m_usingRopePrefab = usingRope;
            bestConnector.GetComponent<RopeGenerator>().GenerateRope(hitObject);

            hitObject.GetComponent<RopeGenerator>().m_prev = bestConnector;
            hitObject.layer = SingletonMaster.Instance.PLAYER_LAYER; // Connected layer number (player)
            m_linkedObjects.Add(hitObject);
            hitBody.gameObject.transform.SetParent(m_linkObjectsParent.transform, true);
            
            // Firing Link Event
            SingletonMaster.Instance.EventManager.LinkEvent.Invoke(hitObject);
            
            // Processing link object specific stuff here ---------------------------------------------
            // var shootComp = hitObject.GetComponent<ShootComponent>();
            // if (shootComp != null)
            // {
            //     shootComp.m_canShoot = true;
            // }
        }
    }

    // For removing linked objects
    public void RemoveLinkedObject(GameObject obj)
    {
        if (m_linkedObjects.Contains(obj))
        {
            RopeGenerator targetRope = obj.GetComponent<RopeGenerator>();

            if (targetRope.m_prev != null)
            {
                // Detaching rope !!
                targetRope.m_prev.GetComponent<RopeGenerator>().DetachRope(obj);

                Debug.Log("Removed " + obj);
                m_linkedObjects.Remove(obj);
            }
        }
    }

    
}
