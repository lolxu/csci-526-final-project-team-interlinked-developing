using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public Rigidbody2D m_RB;
    public float m_acceleration = 50.0f;
    public float m_maxSpeed = 100.0f;
    
    [Header("Rope Settings")]
    public RopeGenerator m_rope;
    public float m_connectRadius = 10.0f;
    public List<GameObject> m_linkedObjects = new List<GameObject>();

    [Header("Visual Settings")] 
    public CinemachineCamera m_cinemachine;
    public float m_cameraZoomFactor = 0.025f;
    public GameObject m_face;
    public float m_faceMoveFactor = 0.5f;
    public GameObject m_gun;

    [Header("Fire Projectile Settings")] 
    public GameObject m_bulletPrefab;
    public float m_fireRate;
    
    private float m_horiVal;
    private float m_vertVal;
    private Vector2 m_drawpos;
    private float m_orgZoom;
    private float m_fireTimeout = 0.0f;

    private void Start()
    {
        // Adding self to linked object list first
        m_linkedObjects.Add(gameObject);
        m_orgZoom = m_cinemachine.Lens.OrthographicSize;
    }

    /// <summary>
    /// For physics based movements
    /// </summary>
    private void FixedUpdate()
    {
        Vector2 moveDir = new Vector2(m_horiVal, m_vertVal);
        Vector3 faceDir = new Vector3(moveDir.x, moveDir.y, 0.0f) * m_faceMoveFactor;
        m_face.transform.localPosition = faceDir;
        if (m_RB.velocity.magnitude < m_maxSpeed)
        {
            m_RB.velocity += moveDir * m_acceleration * Time.fixedDeltaTime;
        }
        
        Vector3 selfToMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float angle = Mathf.Atan2(selfToMouse.y, selfToMouse.x) * Mathf.Rad2Deg;
        m_gun.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
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
        // Getting inputs & stuff...
        m_horiVal = Input.GetAxis("Horizontal");
        m_vertVal = Input.GetAxis("Vertical");

        Vector2 myPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

        // Using mouse clicks to connect components
        if (Input.GetKey(KeyCode.Mouse0))
        {
            // Checking if mouse hits the pickups
            RaycastHit2D pickupHit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 1.0f,
                LayerMask.GetMask("Unconnected"));
            if (pickupHit)
            {
                Rigidbody2D hitBody = pickupHit.rigidbody;
                // Double checking in case weird shit adds this thing twice
                if (!m_linkedObjects.Contains(hitBody.gameObject))
                {
                    RequestRopeConnect(hitBody);
                }
            }
            else
            {
                // Here's the shooty controls
                m_fireTimeout -= Time.deltaTime;
                if (m_fireTimeout <= 0.0f)
                {
                    m_fireTimeout = 60.0f / m_fireRate;
                    GameObject bullet = Instantiate(m_bulletPrefab, transform.position, Quaternion.identity);
                    PlayerBullet bulletScript = bullet.GetComponent<PlayerBullet>();
                    bulletScript.m_direction = (mouseWorldPos - myPos).normalized;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 1.0f,
                LayerMask.GetMask("Connected"));
            if (hit.rigidbody != m_RB && m_linkedObjects.Contains(hit.rigidbody.gameObject))
            {
                // needs some work...
                Debug.Log("Disconnected " + hit.rigidbody.gameObject); 
                RopeGenerator targetRope = hit.rigidbody.gameObject.GetComponent<RopeGenerator>();
                targetRope.m_prev.GetComponent<RopeGenerator>().DetachRope(hit.rigidbody.gameObject);
                
                Debug.Log("Removed " + hit.rigidbody.gameObject);
                m_linkedObjects.Remove(hit.rigidbody.gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        CameraZoomControl();
    }

    private void RequestRopeConnect(Rigidbody2D hitBody)
    {
        GameObject hitObject = hitBody.gameObject;
        m_drawpos = hitBody.position;
        
        // Do a circle cast
        RaycastHit2D[] results = new RaycastHit2D[100];
        int numHit = Physics2D.CircleCastNonAlloc(hitBody.position, m_connectRadius, 
            Vector2.zero, results, m_connectRadius, LayerMask.GetMask("Connected"));
        Debug.Log(numHit);
        
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
            bestConnector.GetComponent<RopeGenerator>().GenerateRope(hitObject);

            hitObject.GetComponent<RopeGenerator>().m_prev = bestConnector;
            hitObject.layer = 7; // Connected layer number
            hitObject.tag = "Player"; // Change tag to player too
            m_linkedObjects.Add(hitObject);
        }
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
        
        m_cinemachine.Lens.OrthographicSize = Vector2.Distance(minCorner, maxCorner) * m_cameraZoomFactor + m_orgZoom;
    }
}
