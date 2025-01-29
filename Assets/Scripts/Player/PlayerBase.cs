using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerBase : MonoBehaviour
{
    [Header("Basic Settings")] 
    public float m_health = 10.0f;
    public PlayerInput m_input;
    public float m_acceleration = 50.0f;
    public float m_maxSpeed = 100.0f;
    public float m_invincibleTime = 1.0f;
    
    [Header("Rope Settings")]
    public RopeGenerator m_rope;
    public float m_connectRadius = 10.0f;
    public List<GameObject> m_linkedObjects = new List<GameObject>();

    [Header("Visual Settings")] 
    public CinemachineVirtualCamera m_cinemachine;
    public float m_cameraZoomFactor = 0.025f;
    public GameObject m_face;
    public float m_faceMoveFactor = 0.5f;
    public GameObject m_gun;
    public Light2D m_muzzleFlash;
    public float m_muzzleFlashIntensity = 10.0f;

    [Header("Fire Projectile Settings")] 
    public GameObject m_bulletPrefab;
    public int m_fireRateChange = 0;
    public float m_recoilChange = 0.0f;
    public int m_penetrationChange = 0;

    [Header("Events")] 
    public UnityEvent<float, GameObject> PlayerDamageEvent = new UnityEvent<float, GameObject>();
    public UnityEvent<GameObject> PlayerDeathEvent = new UnityEvent<GameObject>();
    
    private Rigidbody2D m_RB;
    private SpriteRenderer m_spriteRenderer;
    private Color m_orgColor;
    private Vector3 m_orgScale;

    private bool m_isMouseDown;

    private Vector2 m_moveDirection;
    private Vector2 m_drawpos;
    private float m_orgZoom;
    private float m_fireTimeout = 0.0f;

    private Sequence m_damageTween = null;
    private bool m_isInvincible = false;

    private void Start()
    {
        // Adding self to linked object list first
        m_linkedObjects.Add(gameObject);
        m_orgZoom = m_cinemachine.m_Lens.OrthographicSize;
        m_RB = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_orgColor = m_spriteRenderer.color;
        m_orgScale = transform.localScale;
        
        PlayerDamageEvent.AddListener(OnPlayerDamage);
        PlayerDeathEvent.AddListener(OnPlayerDeath);
    }

    private void OnDisable()
    {
        PlayerDamageEvent.RemoveListener(OnPlayerDamage);
        PlayerDeathEvent.RemoveListener(OnPlayerDeath);
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
        
        Vector3 selfToMouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.value) - transform.position;
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        Vector2 myPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);

        if (m_isMouseDown)
        {
            // Here's the shooty controls
            m_fireTimeout -= Time.deltaTime;
            if (m_fireTimeout <= 0.0f)
            {
                m_muzzleFlash.intensity = m_muzzleFlashIntensity;
                
                GameObject bullet = Instantiate(m_bulletPrefab, transform.position, Quaternion.identity);
                BasePlayerBullet bulletScript = bullet.GetComponent<BasePlayerBullet>();
                
                //TODO: Make sure things don't go negative...
                // Modding stats for bullets
                bulletScript.m_penetrateNum += m_penetrationChange;
                m_fireTimeout = 60.0f / (bulletScript.m_fireRate + m_fireRateChange);
                bulletScript.m_direction = (mouseWorldPos - myPos).normalized;
                
                // Add some recoil;
                m_RB.AddForce(-bulletScript.m_direction * (bulletScript.m_recoil + m_recoilChange), ForceMode2D.Impulse);
            }
            else
            {
                m_muzzleFlash.intensity -= 50.0f * Time.deltaTime;
                if (m_muzzleFlash.intensity < 0.0f)
                {
                    m_muzzleFlash.intensity = 0.0f;
                }
            }
        }
        else
        {
            m_muzzleFlash.intensity = 0.0f;
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
            Vector2.zero, results, m_connectRadius, LayerMask.GetMask("Player"));
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
            hitObject.layer = 7; // Connected layer number (player)
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
        
        m_cinemachine.m_Lens.OrthographicSize = Vector2.Distance(minCorner, maxCorner) * m_cameraZoomFactor + m_orgZoom;
    }
    
    public void Move(InputAction.CallbackContext context)
    {
        Debug.Log("Moving!");

        m_moveDirection = context.ReadValue<Vector2>();
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Fire");
            m_isMouseDown = true;
        }
        else
        {
            m_isMouseDown = false;
        }
    }

    public void RopeOperations(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);

            // Checking if mouse hits the unconnected hit boxes
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

                return;
            }

            // Checking if mouse hits connected loot
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 1.0f,
                LayerMask.GetMask("Player"));

            if (hit && hit.rigidbody != m_RB && m_linkedObjects.Contains(hit.rigidbody.gameObject))
            {
                // needs some work...
                Debug.Log("Disconnected " + hit.rigidbody.gameObject);
                RopeGenerator targetRope = hit.rigidbody.gameObject.GetComponent<RopeGenerator>();

                if (targetRope.m_prev != null)
                {
                    targetRope.m_prev.GetComponent<RopeGenerator>().DetachRope(hit.rigidbody.gameObject);
                    Debug.Log("Removed " + hit.rigidbody.gameObject);
                    m_linkedObjects.Remove(hit.rigidbody.gameObject);
                }
            }
        }
    }

    private void OnPlayerDamage(float damage, GameObject instigator)
    {
        if (!m_isInvincible && m_health > 0)
        {
            // Do some juice stuff here
            if (m_damageTween != null)
            {
                m_damageTween.Kill(true);
            }

            StartCoroutine(InvincibleSequence());
            StartCoroutine(HitStop());
            
            // Juice Stuff
            SingletonMaster.Instance.FeelManager.m_cameraShake.PlayFeedbacks(Vector3.zero, 2.5f);
            m_health -= damage;
            if (m_health <= 0.0f)
            {
                m_damageTween.Kill();
                m_damageTween = DOTween.Sequence();
                m_damageTween.Insert(0, m_spriteRenderer.DOColor(Color.white, 0.1f)
                    .SetLoops(1, LoopType.Yoyo)
                    .SetEase(Ease.InOutFlash));
                m_damageTween.Insert(0,
                    transform.DOPunchScale(transform.localScale * 0.5f, 0.1f));
                m_damageTween.OnComplete(() =>
                {
                    PlayerDeathEvent.Invoke(instigator);
                });
            }
            else
            {
                m_damageTween = DOTween.Sequence();
                m_damageTween.Insert(0, m_spriteRenderer.DOColor(Color.white, 0.1f)
                    .SetLoops((int)(m_invincibleTime / 0.1f), LoopType.Yoyo)
                    .SetEase(Ease.InOutFlash).OnComplete(() => { m_spriteRenderer.color = m_orgColor; }));
                m_damageTween.Insert(0,
                    transform.DOPunchScale(transform.localScale * 0.5f, 0.1f).OnComplete(() =>
                    {
                        transform.localScale = m_orgScale;
                    }));
            }
        }
    }

    private void OnPlayerDeath(GameObject killer)
    {
        m_isInvincible = false;
        Destroy(gameObject);
    }

    private IEnumerator InvincibleSequence()
    {
        m_isInvincible = true;
        yield return new WaitForSeconds(m_invincibleTime);
        m_isInvincible = false;
    }

    private IEnumerator HitStop()
    {
        float orgTimeScale = Time.timeScale;
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = orgTimeScale;
    }
}
