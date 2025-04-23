using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class EnvironmentalRopeComponent : MonoBehaviour
{
    [Header("Core Settings")]
    public List<GameObject> m_connectedTo = new List<GameObject>();
    public List<GameObject> m_receivedFrom = new List<GameObject>();
    public List<GameObject> m_ropeLinksEnvironmental = new List<GameObject>();
    public int m_ropeLength = 20;
    public float m_linkScaleFactor = 0.25f;

    [Tooltip("The rope asset that is using to connect to other objects")]
    public GameObject m_usingRopePrefab;

    private Rigidbody2D m_anchorObject;
    private GameObject m_rope;

    [Header("Visual Settings")] 
    [SerializeField] private LineRenderer m_environmentRopeLine;

    private void Start()
    {
        m_rope = GameObject.FindGameObjectWithTag("Rope");
        m_anchorObject = GetComponent<Rigidbody2D>();

        if (m_connectedTo.Count > 0)
        {
            for (int i = 0; i < m_connectedTo.Count; i++)
            {
                var nextConnected = m_connectedTo[i];
                GenerateRope(nextConnected);
            }
        }
    }

    public void GenerateRope(GameObject connectTo)
    {
        Debug.Log("Generate Rope Connecting to: " + connectTo);
        EnvironmentalRopeComponent rc = connectTo.GetComponent<EnvironmentalRopeComponent>();

        if (rc != null)
        {
            CreatePhysicalRope(connectTo);
        }
    }

    public void ReceiveRope(Rigidbody2D ropeEnd, GameObject instigator)
    {
        EnvironmentalRopeComponent rc = GetComponent<EnvironmentalRopeComponent>();
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

        GetComponent<Rigidbody2D>().totalForce = Vector2.zero;
        transform.position = oldpos;
    }

    private void CreatePhysicalRope(GameObject connectTo)
    {
        Rigidbody2D prevRB = m_anchorObject;
        
        EnvironmentalRopeComponent receiver = connectTo.GetComponent<EnvironmentalRopeComponent>();
        if (receiver != null && connectTo != gameObject)
        {
            var length = receiver.m_ropeLength;

            Vector3 vec = connectTo.transform.position - transform.position;
            Vector3 vecUnit = vec / length;

            if (receiver)
            {
                receiver.m_environmentRopeLine.positionCount = length;
            }
            
            for (int i = 0; i < length; i++)
            {
                // Instantiating a rope link
                GameObject link = Instantiate(m_usingRopePrefab, m_rope.transform, true);
                link.transform.position = transform.position + vecUnit * i;
                
                if (receiver.m_ropeLinksEnvironmental.Count < length)
                {
                    receiver.m_ropeLinksEnvironmental.Add(link);
                }
                
                // Setting up joints
                HingeJoint2D joint = link.GetComponent<HingeJoint2D>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = Vector2.down * m_linkScaleFactor;
                joint.connectedBody = prevRB;
                
                
                if (i == 0) // First one
                {
                    joint.autoConfigureConnectedAnchor = false;
                    joint.connectedAnchor = Vector2.zero;
                    
                    HingeJoint2D myJoint = gameObject.AddComponent<HingeJoint2D>();
                    myJoint.connectedBody = link.GetComponent<Rigidbody2D>();
                    
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

    // Updating rope line renderer
    private void Update()
    {
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
}
