using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class RopeComponent : MonoBehaviour
{
    [Header("Core Settings")]
    public List<GameObject> m_connectedTo = new List<GameObject>();
    public List<GameObject> m_receivedFrom = new List<GameObject>();
    public List<GameObject> m_ropeLinksEnemy = new List<GameObject>();
    public List<GameObject> m_ropeLinksPlayer = new List<GameObject>();
    public List<GameObject> m_ropeLinksEnvironmental = new List<GameObject>();
    public int m_ropeLength = 20;
    public bool m_isConnectedToPlayer = false;
    public bool m_isConnectedToEnemy = false;
    public bool m_isEnvironmental = false;
    public bool m_isEnemy;
    // public bool m_isStrongConnection = false;

    [Tooltip("The rope asset that is using to connect to other objects")]
    public GameObject m_usingRopePrefab;

    [SerializeField] private HingeJoint2D m_enemyJoint;
    [SerializeField] private HingeJoint2D m_playerJoint;
    private GameObject m_enemyObject;
    private Rigidbody2D m_anchorObject;
    private GameObject m_rope;

    // [Header("Rope Physics Settings")] 
    private float m_maxTension = 200.0f;
    private float m_tensionDuration = 0.75f;

    private Color m_orgEnemyRopeColor;
    private float m_ropeStressTimer = 0.0f;
    private bool m_isStealing = false;

    [Header("Visual Settings")] 
    [SerializeField] private LineRenderer m_playerRopeLine;
    [SerializeField] private LineRenderer m_enemyRopeLine;
    [SerializeField] private LineRenderer m_environmentRopeLine;
    [SerializeField] private GameObject m_highlight;
    [SerializeField] private Color m_connectColor = Color.white;
    [SerializeField] private Color m_disconnectColor = Color.cyan;
    [SerializeField] private float m_colorAlpha = 0.5f;

    private IEnumerator Start()
    {
        if (m_highlight != null)
        {
            m_highlight.SetActive(false);
            m_connectColor.a = m_colorAlpha;
            m_disconnectColor.a = m_colorAlpha;
        }
        
        m_rope = GameObject.FindGameObjectWithTag("Rope");
        m_anchorObject = GetComponent<Rigidbody2D>();

        if (SingletonMaster.Instance != null)
        {
            SingletonMaster.Instance.EventManager.StealSuccessEvent.AddListener(OnStealSuccess);
            SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);
        }

        yield return null;
        
        if (gameObject.CompareTag("Enemy"))
        {
            m_isEnemy = true;
        }
        
        if (m_connectedTo.Count > 0)
        {
            for (int i = 0; i < m_connectedTo.Count; i++)
            {
                var nextConnected = m_connectedTo[i];
                GenerateRope(nextConnected);
            }
        }
        
        // Coloring
        if (m_enemyRopeLine != null)
        {
            m_orgEnemyRopeColor = m_enemyRopeLine.startColor;
        }
    }

    private void OnDisable()
    {
        if (SingletonMaster.Instance != null)
        {
            SingletonMaster.Instance.EventManager.StealSuccessEvent.RemoveListener(OnStealSuccess);
            SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
        }
    }
    
    private void OnPlayerDeath(GameObject obj)
    {
        if (m_highlight != null)
        {
            HideHighlight();
        }
    }

    private void OnStealSuccess(GameObject obj, GameObject enemy)
    {
        if (obj == gameObject)
        {
            DetachEnemy(enemy);
        }
    }

    public void GenerateRope(GameObject connectTo)
    {
        Debug.Log("Generate Rope Connecting to: " + connectTo);
        RopeComponent rc = connectTo.GetComponent<RopeComponent>();

        if (rc != null)
        {
            // Checking if I am player or not
            if (gameObject.CompareTag("Player"))
            {
                rc.m_isConnectedToPlayer = true;
            }
            else if (gameObject.CompareTag("Enemy"))
            {
                rc.m_isConnectedToEnemy = true;
                rc.m_enemyObject = gameObject;
            }

            CreatePhysicalRope(connectTo);
        }
    }

    private void OnJointBreak2D(Joint2D brokenJoint)
    {
        Debug.Log("ROPE BROKE!!");
        if (brokenJoint == m_enemyJoint && !m_isEnemy)
        {
            m_isConnectedToEnemy = false;
            m_isStealing = false;
            SingletonMaster.Instance.EventManager.StealSuccessEvent.Invoke(gameObject, m_enemyObject);
            SingletonMaster.Instance.EventManager.StealEndedEvent.Invoke(gameObject, m_enemyObject);
        }
        else if (brokenJoint == m_enemyJoint && m_isEnemy)
        {
            // Call detach enemy on all connected items
            for (int i = m_connectedTo.Count - 1; i >= 0; --i)
            {
                GameObject connected = m_connectedTo[i];
                RopeComponent connectedRC = connected.GetComponent<RopeComponent>();
                if (connectedRC != null)
                {
                    SingletonMaster.Instance.EventManager.StealSuccessEvent.Invoke(connected, gameObject);
                }
            }

            if (m_receivedFrom.Count > 0)
            {
                m_isConnectedToEnemy = false;
                m_isStealing = false;
                SingletonMaster.Instance.EventManager.StealSuccessEvent.Invoke(gameObject, m_enemyObject);
                SingletonMaster.Instance.EventManager.StealEndedEvent.Invoke(gameObject, m_enemyObject);
            }

            if (m_enemyJoint != null)
            {
                Destroy(m_enemyJoint);
            }
        }
        else // We tossing enemies (not sure if we want this yet...)
        {
            var rc = brokenJoint.gameObject.GetComponent<RopeComponent>();
            if (rc != null)
            {
                rc.DetachRope(SingletonMaster.Instance.PlayerBase.gameObject);
            }
        }
    }

    public void ReceiveRope(Rigidbody2D ropeEnd, GameObject instigator)
    {
        RopeComponent rc = GetComponent<RopeComponent>();
        if (rc != null)
        {
            Debug.Log("Received Connection From: " + instigator);
            rc.m_receivedFrom.Add(instigator);
        }
        else
        {
            Debug.LogError("Receiver doesn't have RopeComponent!!!");
        }
        
        Vector3 oldpos = transform.position;
        HingeJoint2D joint = gameObject.AddComponent<HingeJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = ropeEnd;
        joint.anchor = Vector2.zero;
        joint.connectedAnchor = Vector2.zero;

        if (instigator.CompareTag("Enemy"))
        {
            m_enemyJoint = joint;
        }
        else if (instigator.CompareTag("Player"))
        {
            m_playerJoint = joint;
        }

        GetComponent<Rigidbody2D>().totalForce = Vector2.zero;
        transform.position = oldpos;
        
        // Checking if connected to both enemy and player
        if (m_isConnectedToEnemy && m_isConnectedToPlayer)
        {
            SingletonMaster.Instance.EventManager.StealStartedEvent.Invoke(gameObject, m_enemyObject);
            m_isStealing = true;
        }
    }

    private void CreatePhysicalRope(GameObject connectTo)
    {
        Rigidbody2D prevRB = m_anchorObject;
        
        RopeComponent receiver = connectTo.GetComponent<RopeComponent>();
        if (receiver != null && connectTo != gameObject)
        {
            var length = receiver.m_ropeLength;

            Vector3 vec = connectTo.transform.position - transform.position;
            Vector3 vecUnit = vec / length;

            if (receiver.m_isConnectedToPlayer)
            {
                receiver.m_playerRopeLine.positionCount = length;
            }

            if (receiver.m_isConnectedToEnemy)
            {
                receiver.m_enemyRopeLine.positionCount = length;
            }

            if (receiver.m_isEnvironmental)
            {
                receiver.m_environmentRopeLine.positionCount = length;
            }
            
            for (int i = 0; i < length; i++)
            {
                // Instantiating a rope link
                GameObject link = Instantiate(m_usingRopePrefab, m_rope.transform, true);
                link.transform.position = transform.position + vecUnit * i;
                
                if (receiver.m_isConnectedToPlayer && receiver.m_ropeLinksPlayer.Count < length)
                {
                    // Debug.Log("Add Player rope");
                    receiver.m_ropeLinksPlayer.Add(link); // Store rope assets to the receiving end (player)
                }
                if (receiver.m_isConnectedToEnemy && receiver.m_ropeLinksEnemy.Count < length)
                {
                    // Debug.Log("Add Enemy rope");
                    receiver.m_ropeLinksEnemy.Add(link); // Store rope assets to the receiving end (enemy)
                }
                if (receiver.m_isEnvironmental && receiver.m_ropeLinksEnvironmental.Count < length)
                {
                    receiver.m_ropeLinksEnvironmental.Add(link);
                }
                
                // Setting up joints
                HingeJoint2D joint = link.GetComponent<HingeJoint2D>();
                joint.connectedBody = prevRB;
                joint.autoConfigureConnectedAnchor = true;
                
                if (i == 0) // First one
                {
                    joint.autoConfigureConnectedAnchor = false;
                    joint.connectedAnchor = Vector2.zero;
                    
                    HingeJoint2D myJoint = gameObject.AddComponent<HingeJoint2D>();
                    myJoint.connectedBody = link.GetComponent<Rigidbody2D>();
                    
                    // If I am enemy, this is my enemy joint
                    if (m_isEnemy)
                    {
                        m_enemyJoint = myJoint;
                    }
                    
                    prevRB = link.GetComponent<Rigidbody2D>();
                }
                else if (i == length - 1) // Last one
                {
                    receiver.ReceiveRope(link.GetComponent<Rigidbody2D>(), gameObject);
                }
                else // Middle - change previous RB to last link
                {
                    prevRB = link.GetComponent<Rigidbody2D>();
                }
            }
            
            // Firing Link Event ---------------------------------------------------
            SingletonMaster.Instance.EventManager.LinkEvent.Invoke(connectTo, gameObject);
            
            // Checking
            if (!m_connectedTo.Contains(connectTo))
            {
                m_connectedTo.Add(connectTo);
            }
            
        }
        else
        {
            Debug.LogError("Connecting to myself or receiver doesn't have RopeComponent");
        }
        
    }

    public void DetachRope(GameObject instigator)
    {
        // Destroying all rope links to and from this object
        if (instigator.CompareTag("Player"))
        {
            for (int i = m_ropeLinksPlayer.Count - 1; i >= 0; --i)
            {
                Destroy(m_ropeLinksPlayer[i]);
            }
            m_ropeLinksPlayer.Clear();
            m_playerRopeLine.positionCount = 0;
            
            for (int i = m_receivedFrom.Count - 1; i >= 0; --i)
            {
                if (m_receivedFrom[i].CompareTag("Player"))
                {
                    RopeComponent rc = m_receivedFrom[i].GetComponent<RopeComponent>();
                    rc.m_connectedTo.Remove(gameObject);
                    m_receivedFrom.RemoveAt(i);
                }
            }
            
            // Firing Unlink Event ------------------------------------------------
            SingletonMaster.Instance.EventManager.UnlinkEvent.Invoke(gameObject, instigator);

            if (m_isConnectedToEnemy)
            {
                SingletonMaster.Instance.EventManager.StealEndedEvent.Invoke(gameObject, m_enemyObject);
            }
            
            m_isConnectedToPlayer = false;
            m_isStealing = false;
            
            Destroy(m_playerJoint);
        }
    }

    /// <summary>
    /// Call this on receivers of rope!!
    /// </summary>
    /// <param name="enemy">Connected enemy</param>
    public void DetachEnemy(GameObject enemy)
    {
        Debug.Log("Detaching Enemy: " + enemy);

        for (int i = m_ropeLinksEnemy.Count - 1; i >= 0; --i)
        {
            Destroy(m_ropeLinksEnemy[i]);
        }
        m_ropeLinksEnemy.Clear();
        m_enemyRopeLine.positionCount = 0;

        if (!gameObject.CompareTag("Enemy"))
        {
            transform.SetParent(null, true);
        }
        
        SingletonMaster.Instance.EventManager.UnlinkEvent.Invoke(gameObject, enemy);
        
        for (int i = m_receivedFrom.Count - 1; i >= 0; --i)
        {
            if (m_receivedFrom[i] == enemy)
            {
                RopeComponent rc = m_receivedFrom[i].GetComponent<RopeComponent>();
                rc.m_connectedTo.Remove(gameObject);
                m_receivedFrom.RemoveAt(i);
            }
        }

        m_isConnectedToEnemy = false;
        Destroy(m_enemyJoint);
    }

    // Updating rope line renderer
    private void Update()
    {
        if (m_playerRopeLine != null)
        {
            // Updating player ropes
            var playerPoints = new Vector3[m_playerRopeLine.positionCount];
            for (int i = 0; i < m_playerRopeLine.positionCount; i++)
            {
                playerPoints[i] = m_ropeLinksPlayer[i].transform.position;
            }

            m_playerRopeLine.SetPositions(playerPoints);
        }

        if (m_enemyRopeLine != null)
        {
            // Updating enemy ropes
            var enemyPoints = new Vector3[m_enemyRopeLine.positionCount];
            for (int i = 0; i < m_enemyRopeLine.positionCount; i++)
            {
                enemyPoints[i] = m_ropeLinksEnemy[i].transform.position;
            }

            m_enemyRopeLine.SetPositions(enemyPoints);
        }
        
        if (m_environmentRopeLine != null)
        {
            // Updating environmental ropes
            var envPoints = new Vector3[m_environmentRopeLine.positionCount];
            for (int i = 0; i < m_environmentRopeLine.positionCount; i++)
            {
                envPoints[i] = m_ropeLinksEnvironmental[i].transform.position;
            }

            m_environmentRopeLine.SetPositions(envPoints);
        }

    }

    private void FixedUpdate()
    {
        // if (gameObject.CompareTag("Enemy"))
        {
            // Do the stress mechanic when player is stealing
            // OR
            // when player is connected to enemy that has something
            if ((m_enemyJoint != null && m_isStealing) || (m_enemyJoint != null && m_isConnectedToPlayer))
            {
                float stress = m_enemyJoint.GetReactionForce(Time.fixedDeltaTime).magnitude;
                Debug.Log(stress);
                if (stress > m_maxTension)
                {
                    // Debug.Log("Adding stress");
                    m_ropeStressTimer += Time.fixedDeltaTime;
                    
                    Color tmp = m_enemyRopeLine.startColor;
                    tmp.r += 0.75f * Time.fixedDeltaTime;
                    tmp.r = Mathf.Clamp01(tmp.r);

                    m_enemyRopeLine.startColor = tmp;
                    m_enemyRopeLine.endColor = tmp;
                    
                    // Also go through all enemy connected items
                    foreach (var connected in m_connectedTo)
                    {
                        RopeComponent connectedRC = connected.GetComponent<RopeComponent>();
                        connectedRC.m_enemyRopeLine.startColor = tmp;
                        connectedRC.m_enemyRopeLine.endColor = tmp;
                    }

                    if (m_ropeStressTimer > m_tensionDuration)
                    {
                        // Debug.Log("SET BROKEN");
                        m_enemyJoint.breakForce = 10.0f;
                    }
                }
                else
                {
                    // Debug.Log("Resetting stress");
                    m_ropeStressTimer = 0.0f;
                    
                    m_enemyRopeLine.startColor = m_orgEnemyRopeColor;
                    m_enemyRopeLine.endColor = m_orgEnemyRopeColor;
                    
                    // Also go through all enemy connected items
                    foreach (var connected in m_connectedTo)
                    {
                        RopeComponent connectedRC = connected.GetComponent<RopeComponent>();
                        connectedRC.m_ropeStressTimer = 0.0f;
                        connectedRC.m_enemyRopeLine.startColor = m_orgEnemyRopeColor;
                        connectedRC.m_enemyRopeLine.endColor = m_orgEnemyRopeColor;
                    }
                }
            }
            else if (m_enemyJoint != null && !m_isConnectedToPlayer)
            {
                m_ropeStressTimer = 0.0f;
                    
                // Also go through all enemy connected items
                foreach (var connected in m_connectedTo)
                {
                    RopeComponent connectedRC = connected.GetComponent<RopeComponent>();
                    if (!connectedRC.m_isStealing)
                    {
                        connectedRC.m_ropeStressTimer = 0.0f;
                        connectedRC.m_enemyRopeLine.startColor = m_orgEnemyRopeColor;
                        connectedRC.m_enemyRopeLine.endColor = m_orgEnemyRopeColor;
                    }
                }
            }
            else
            {
                // Debug.Log("Resetting stress");
                m_ropeStressTimer = 0.0f;
            }
        }
    }

    
    public void ShowHighlight(bool isConnection)
    {
        m_highlight.GetComponent<SpriteRenderer>().color = isConnection ? m_connectColor : m_disconnectColor;
        m_highlight.SetActive(true);
    }
    
    public void HideHighlight()
    {
        m_highlight.SetActive(false);
    }
}
