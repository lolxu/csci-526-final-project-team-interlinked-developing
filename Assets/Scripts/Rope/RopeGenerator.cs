using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RopeGenerator : MonoBehaviour
{
    public Rigidbody2D m_anchorObject;
    
    [Tooltip("The rope asset that is used to connect to this object")]
    public GameObject m_myRopePrefab;
    
    [Tooltip("The rope asset that is using to connect to other objects")]
    public GameObject m_usingRopePrefab;
    public List<GameObject> m_next = new List<GameObject>();
    public GameObject m_prev;

    private GameObject m_rope;

    private void Start()
    {
        m_rope = GameObject.FindGameObjectWithTag("Rope");
    }

    public void GenerateRope(GameObject connectTo)
    {
        if (connectTo.GetComponent<RelativeJoint2D>() != null)
        {
            Vector2 offset = transform.position - connectTo.transform.position;
            RelativeJoint2D joint = connectTo.GetComponent<RelativeJoint2D>();
            joint.enabled = true;
            joint.connectedBody = m_anchorObject;
            joint.autoConfigureOffset = false;
            joint.linearOffset = offset;
        }
        
        CreateVisualRope(connectTo);
    }

    private void CreateVisualRope(GameObject connectTo)
    {
        Rigidbody2D prevRB = m_anchorObject;
        var receiver = connectTo.GetComponent<RopeReceiver>();
        if (receiver != null)
        {
            var length = receiver.m_length;
            for (int i = 0; i < length; i++)
            {
                // Instantiating a rope link and randomizing scale
                GameObject link = Instantiate(m_usingRopePrefab, m_rope.transform, true);
                link.transform.position = transform.position;
                float randScale = Random.Range(0.5f, 0.5f);
                link.transform.localScale = new Vector3(randScale, randScale, randScale);
            
                HingeJoint2D joint = link.GetComponent<HingeJoint2D>();
                
                joint.connectedBody = prevRB;
                joint.autoConfigureConnectedAnchor = false;
            
                // Determining the connection offset
                // Making it randomized in the middle
                if (i == 0)
                {
                    joint.connectedAnchor = Vector2.zero;
                }
                
                if (i == length - 1)
                {
                    var connector = connectTo.GetComponent<RopeReceiver>();
                    if (connector != null)
                    {
                        connector.ConnectToRope(link.GetComponent<Rigidbody2D>());
                    }
                }
                else
                {
                    prevRB = link.GetComponent<Rigidbody2D>();
                }
            
                connectTo.GetComponent<RopeReceiver>().m_links.Add(link);
            }
        }
        else
        {
            Debug.LogError("Connecting component does NOT have Rope Receiver");
        }
    }

    /// <summary>
    /// This is a function to detach this specific target component from the rope
    /// </summary>
    /// <param name="disconnectTarget"></param>
    public void DetachRope(GameObject disconnectTarget)
    {
        for (int i = m_next.Count - 1; i >= 0; i--)
        {
            GameObject nextObject = m_next[i];
            
            if (disconnectTarget == nextObject)
            {
                RelativeJoint2D strongJoint = nextObject.GetComponent<RelativeJoint2D>();
                if (strongJoint != null)
                {
                    strongJoint.connectedBody = null;
                    strongJoint.enabled = false;
                }
                nextObject.transform.SetParent(null, true);
                
                RopeGenerator nextRope = nextObject.GetComponent<RopeGenerator>();
                RopeReceiver nextConn = nextObject.GetComponent<RopeReceiver>();
                if (nextRope != null && nextConn != null)
                {
                    nextObject.layer = SingletonMaster.Instance.UNCONNECTED_LAYER;
                    
                    nextRope.m_prev = null;
                    nextConn.DestroyRope();
                    nextRope.DetachAll();
                }
                m_next.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    /// This is a function that detach all stuff connected to this current segment in rope...
    /// </summary>
    public void DetachAll()
    {
        
        if (GetComponent<HingeJoint2D>() != null)
        {
            HingeJoint2D myJoint = GetComponent<HingeJoint2D>();
            Destroy(myJoint);
        }
        
        for (int i = m_next.Count - 1; i >= 0; i--)
        {
            GameObject nextObject = m_next[i];
            
            RelativeJoint2D strongJoint = nextObject.GetComponent<RelativeJoint2D>();
            if (strongJoint != null)
            {
                strongJoint.connectedBody = null;
                strongJoint.enabled = false;
            }
            nextObject.transform.SetParent(null, true);
            
            RopeGenerator nextRope = nextObject.GetComponent<RopeGenerator>();
            RopeReceiver nextConn = nextObject.GetComponent<RopeReceiver>();
            if (nextRope != null && nextConn != null)
            {
                nextObject.layer = SingletonMaster.Instance.UNCONNECTED_LAYER;
                
                nextRope.m_prev = null;
                nextConn.DestroyRope();
                
                if (GetComponent<HingeJoint2D>() != null)
                {
                    HingeJoint2D nextJoint = GetComponent<HingeJoint2D>();
                    Destroy(nextJoint);
                }
                else if (GetComponent<RelativeJoint2D>() != null)
                {
                    RelativeJoint2D nextJoint = GetComponent<RelativeJoint2D>();
                    Destroy(nextJoint);
                }
                
                nextRope.DetachAll();
            }

            SingletonMaster.Instance.PlayerBase.m_linkedObjects.Remove(nextObject);
            m_next.RemoveAt(i);
        }
    }
    
}
