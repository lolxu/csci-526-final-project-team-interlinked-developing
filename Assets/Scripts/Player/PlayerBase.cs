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
    public float m_health = 10.0f;
    public PlayerInput m_input;
    public float m_acceleration = 50.0f;
    public float m_maxSpeed = 100.0f;
    public float m_invincibleTime = 1.0f;
    
    [Header("Rope Settings")]
    public GameObject m_rope;
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

    // private Sequence m_damageTween = null;
    private Coroutine m_damageSequence = null;
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
        
        SingletonMaster.Instance.EventManager.PlayerDamageEvent.AddListener(OnPlayerDamage);
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        m_linkedDisplacements.Clear();
        m_ropeDisplacements.Clear();
        
        foreach (var obj in m_linkedObjects)
        {
            Vector3 disp = obj.transform.position - transform.position;
            m_linkedDisplacements.Add(disp);
            obj.GetComponent<Rigidbody2D>().isKinematic = true;
            Debug.Log(disp);
        }

        for (int i = 0; i < m_rope.transform.childCount; i++)
        {
            Rigidbody2D rb = m_rope.transform.GetChild(i).gameObject.GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            Vector3 disp = m_rope.transform.GetChild(i).position - transform.position;
            m_ropeDisplacements.Add(disp);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        transform.position = Vector3.zero;
        StartCoroutine(InitCoroutine());
    }

    private IEnumerator InitCoroutine()
    {
        // yield return new WaitForSeconds(0.5f);
        yield return null;
        for (int i = 0; i < m_linkedObjects.Count; i++)
        {
            Debug.Log(m_linkedDisplacements[i]);
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

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.PlayerDamageEvent.RemoveListener(OnPlayerDamage);
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        CameraZoomControl();
    }

    private void RequestRopeConnect(Rigidbody2D hitBody, GameObject usingRope)
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
            bestConnector.GetComponent<RopeGenerator>().m_usingRopePrefab = usingRope;
            bestConnector.GetComponent<RopeGenerator>().GenerateRope(hitObject);

            hitObject.GetComponent<RopeGenerator>().m_prev = bestConnector;
            hitObject.layer = SingletonMaster.Instance.PLAYER_LAYER; // Connected layer number (player)
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
        m_moveDirection = context.ReadValue<Vector2>();
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
                    GameObject usingRope = hitBody.gameObject.GetComponent<RopeGenerator>().m_myRopePrefab;
                    
                    hitBody.gameObject.transform.SetParent(m_linkObjectsParent.transform, true);
                    RequestRopeConnect(hitBody, usingRope);
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
                    // Detaching rope !!
                    targetRope.m_prev.GetComponent<RopeGenerator>().DetachRope(hit.rigidbody.gameObject);
                    
                    Debug.Log("Removed " + hit.rigidbody.gameObject);
                    m_linkedObjects.Remove(hit.rigidbody.gameObject);
                }
            }
        }
    }

    private void OnPlayerDamage(float damage, GameObject instigator)
    {
        if (!m_isInvincible && m_health > 0.0f)
        {
            // Do some juice stuff here
            // if (m_damageTween != null)
            // {
            //     m_damageTween.Kill(true);
            // }
            
            if (m_damageSequence != null)
            {
                // StopCoroutine(m_damageSequence);
                // m_spriteRenderer.color = m_orgColor;
                // transform.localScale = m_orgScale;
            }

            Vector2 dir = -(instigator.transform.position - transform.position).normalized;
            m_damageSequence = StartCoroutine(PlayerHurtSequence(dir));

            StartCoroutine(InvincibleSequence());
            StartCoroutine(HitStop());
            
            // Juice Stuff
            // SingletonMaster.Instance.FeelManager.m_cameraShake.PlayFeedbacks(Vector3.zero, 2.5f);
            SingletonMaster.Instance.CameraShakeManager.Shake(10.0f, 0.25f);
            m_health -= damage;
            if (m_health <= 0.0f)
            {
                Time.timeScale = 1.0f;
                StopAllCoroutines();
                // m_damageTween.Kill();
                // m_damageTween = DOTween.Sequence();
                // m_damageTween.Insert(0, m_spriteRenderer.DOColor(Color.white, 0.1f)
                //     .SetLoops(1, LoopType.Yoyo)
                //     .SetEase(Ease.InOutFlash));
                // m_damageTween.Insert(0,
                //     transform.DOPunchScale(transform.localScale * 0.5f, 0.1f));
                // m_damageTween.OnComplete(() =>
                // {
                //     SingletonMaster.Instance.EventManager.PlayerDeathEvent.Invoke(instigator);
                // });
                
                SingletonMaster.Instance.EventManager.PlayerDeathEvent.Invoke(instigator);
            }
            else
            {
                // m_damageTween = DOTween.Sequence();
                // m_damageTween.Insert(0, m_spriteRenderer.DOColor(Color.white, 0.1f)
                //     .SetLoops((int)(m_invincibleTime / 0.1f), LoopType.Yoyo)
                //     .SetEase(Ease.InOutFlash).OnComplete(() => { m_spriteRenderer.color = m_orgColor; }));
                // m_damageTween.Insert(0,
                //     transform.DOPunchScale(transform.localScale * 0.5f, 0.1f).OnComplete(() =>
                //     {
                //         transform.localScale = m_orgScale;
                //     }));
            }
        }
    }
    
    private IEnumerator PlayerHurtSequence(Vector2 dir)
    {
        float flashDuration = 0.0f;
        m_RB.AddForce(dir * 300.0f, ForceMode2D.Impulse);
        while (flashDuration <= m_invincibleTime)
        {
            m_spriteRenderer.color = Color.white;
            yield return new WaitForSecondsRealtime(0.1f);
            m_spriteRenderer.color = m_orgColor;
            yield return new WaitForSecondsRealtime(0.1f);
            flashDuration += 0.2f;
        }
        m_spriteRenderer.color = m_orgColor;
    }

    private void OnPlayerDeath(GameObject killer)
    {
        m_isInvincible = false;
        Destroy(gameObject);
    }

    private IEnumerator InvincibleSequence()
    {
        m_isInvincible = true;
        yield return new WaitForSecondsRealtime(m_invincibleTime);
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
