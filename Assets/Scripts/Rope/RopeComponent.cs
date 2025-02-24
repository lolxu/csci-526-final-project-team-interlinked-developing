using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class RopeComponent : MonoBehaviour
{
    public List<GameObject> m_connectedTo = new List<GameObject>();
    public List<GameObject> m_receivedFrom = new List<GameObject>();
    public List<GameObject> m_ropeLinksEnemy = new List<GameObject>();
    public List<GameObject> m_ropeLinksPlayer = new List<GameObject>();
    public int m_ropeLength = 20;
    public bool m_isConnectedToPlayer = false;
    public bool m_isConnectedToEnemy = false;
    // public bool m_isStrongConnection = false;
    
    [Tooltip("The rope asset that is using to connect to other objects")]
    public GameObject m_usingRopePrefab;

    private HingeJoint2D m_enemyJoint;
    private HingeJoint2D m_playerJoint;
    private RelativeJoint2D m_enemyStrongJoint;
    private RelativeJoint2D m_playerStrongJoint;
    private GameObject m_enemyObject;
    private Rigidbody2D m_anchorObject;
    private GameObject m_rope;
    
    [Header("Enemy Rope Settings")]
    [SerializeField] private Color m_orgEnemyRopeColor;
    private float m_ropeStressTimer = 0.0f;
    private bool m_isStealing = false;

    private IEnumerator Start()
    { 
        m_rope = GameObject.FindGameObjectWithTag("Rope");
        m_anchorObject = GetComponent<Rigidbody2D>();
        
        SingletonMaster.Instance.EventManager.StealSuccessEvent.AddListener(OnStealSuccess);

        yield return null;
        
        if (m_connectedTo.Count > 0)
        {
            for (int i = 0; i < m_connectedTo.Count; i++)
            {
                var nextConnected = m_connectedTo[i];
                GenerateRope(nextConnected);
            }
        }
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.StealSuccessEvent.RemoveListener(OnStealSuccess);
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
            // No strong connection just yet... 
            //
            // Making strong connection if the connectTo demands it
            // RelativeJoint2D strongJoint = null;
            // if (rc.m_isStrongConnection)
            // {
            //     strongJoint = connectTo.AddComponent<RelativeJoint2D>();
            //     strongJoint.enabled = true;
            //     strongJoint.connectedBody = m_anchorObject;
            //     strongJoint.autoConfigureOffset = true;
            //     strongJoint.maxForce = 5000.0f;
            //     strongJoint.maxTorque = 2500.0f;
            // }
            
            // Checking if I am player or not
            if (gameObject.CompareTag("Player"))
            {
                // if (strongJoint != null)
                // {
                //     rc.m_playerStrongJoint = strongJoint;
                // }
                rc.m_isConnectedToPlayer = true;
            }
            else if (gameObject.CompareTag("Enemy"))
            {
                // if (strongJoint != null)
                // {
                //     rc.m_enemyStrongJoint = strongJoint;
                // }
                rc.m_isConnectedToEnemy = true;
                rc.m_enemyObject = gameObject;
            }

            CreatePhysicalRope(connectTo);
        }
    }

    private void OnJointBreak2D(Joint2D brokenJoint)
    {
        if (brokenJoint == m_enemyJoint)
        {
            m_isConnectedToEnemy = false;
            m_isStealing = false;
            SingletonMaster.Instance.EventManager.StealSuccessEvent.Invoke(gameObject, m_enemyObject);
            SingletonMaster.Instance.EventManager.StealEndedEvent.Invoke(gameObject, m_enemyObject);
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
            rc.m_receivedFrom.Add(instigator);
        }
        
        Vector3 oldpos = transform.position;
        HingeJoint2D joint = gameObject.AddComponent<HingeJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = ropeEnd;
        joint.anchor = Vector2.zero;
        joint.connectedAnchor = Vector2.zero;
        
        // Make weak joints for enemies
        if (gameObject.CompareTag("Enemy"))
        {
            // joint.breakForce = 800.0f;
        }

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
                
                // Setting up joints
                HingeJoint2D joint = link.GetComponent<HingeJoint2D>();
                joint.connectedBody = prevRB;
                joint.autoConfigureConnectedAnchor = true;
                
                if (i == 0) // First one
                {
                    joint.autoConfigureConnectedAnchor = false;
                    joint.connectedAnchor = Vector2.zero;
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
            Debug.LogError("Connecting component does NOT have Rope Receiver");
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
            
            for (int i = m_receivedFrom.Count - 1; i >= 0; --i)
            {
                if (m_receivedFrom[i].CompareTag("Player"))
                {
                    RopeComponent rc = m_receivedFrom[i].GetComponent<RopeComponent>();
                    rc.m_connectedTo.Remove(gameObject);
                }
            }
            m_receivedFrom.Clear();
            
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

    public void DetachEnemy(GameObject enemy)
    {
        for (int i = m_ropeLinksEnemy.Count - 1; i >= 0; --i)
        {
            Destroy(m_ropeLinksEnemy[i]);
        }
        m_ropeLinksEnemy.Clear();
        
        transform.SetParent(null, true);
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

    private void FixedUpdate()
    {
        // if (gameObject.CompareTag("Enemy"))
        {
            // var joint = GetComponent<HingeJoint2D>();
            if (m_enemyJoint != null && m_isStealing)
            {
                // Debug.Log(joint.GetReactionForce(Time.fixedDeltaTime).magnitude);
                float stress = m_enemyJoint.GetReactionForce(Time.fixedDeltaTime).magnitude;
                if (stress > 500.0f)
                {
                    Debug.Log("Adding stress");
                    m_ropeStressTimer += Time.fixedDeltaTime;

                    foreach (var rope in m_ropeLinksEnemy)
                    {
                        var sp = rope.GetComponent<SpriteRenderer>();
                        if (sp != null)
                        {
                            Color tmp = sp.color;
                            tmp.r += 0.75f * Time.fixedDeltaTime;
                            tmp.r = Mathf.Clamp01(tmp.r);
                            sp.color = tmp;
                        }
                    }

                    if (m_ropeStressTimer > 1.0f)
                    {
                        Debug.Log("SET BROKEN");
                        m_enemyJoint.breakForce = 10.0f;
                    }
                }
                else
                {
                    Debug.Log("Resetting stress");
                    m_ropeStressTimer = 0.0f;
                    
                    foreach (var rope in m_ropeLinksEnemy)
                    {
                        var sp = rope.GetComponent<SpriteRenderer>();
                        if (sp != null)
                        {
                            sp.color = m_orgEnemyRopeColor;
                        }
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
}
