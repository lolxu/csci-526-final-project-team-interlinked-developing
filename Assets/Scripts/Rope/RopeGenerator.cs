using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RopeGenerator : MonoBehaviour
{
    public GameObject m_rope;
    public Rigidbody2D m_anchorObject;
    public GameObject m_linkPrefab;
    public List<GameObject> m_next = new List<GameObject>();
    public GameObject m_prev;
    public int m_length = 10;

    public void GenerateRope(GameObject connectTo)
    {
        Rigidbody2D prevRB = m_anchorObject;
        
        for (int i = 0; i < m_length; i++)
        {
            // Instantiating a rope link and randomizing scale
            GameObject link = Instantiate(m_linkPrefab, m_rope.transform, true);
            link.transform.position = transform.position;
            float randScale = Random.Range(0.25f, 0.5f);
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
            else
            {
                joint.connectedAnchor = Random.insideUnitCircle * 0.75f;
            }
            
            // joint.distance = m_ropeOffset;
            if (i == m_length - 1)
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
                RopeGenerator nextRope = nextObject.GetComponent<RopeGenerator>();
                RopeReceiver nextConn = nextObject.GetComponent<RopeReceiver>();
                if (nextRope != null && nextConn != null)
                {
                    nextObject.tag = "Untagged";
                    nextObject.layer = 6;
                    
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
    /// This is a shit function that detach all stuff connected to this current segment in rope...
    /// </summary>
    public void DetachAll()
    {
        HingeJoint2D myJoint = GetComponent<HingeJoint2D>();
        if (myJoint)
        {
            Destroy(myJoint);
        }
        
        for (int i = m_next.Count - 1; i >= 0; i--)
        {
            GameObject nextObject = m_next[i];
            RopeGenerator nextRope = nextObject.GetComponent<RopeGenerator>();
            RopeReceiver nextConn = nextObject.GetComponent<RopeReceiver>();
            if (nextRope != null && nextConn != null)
            {
                nextObject.tag = "Untagged";
                nextObject.layer = 6;
                
                nextRope.m_prev = null;
                nextConn.DestroyRope();
                
                HingeJoint2D nextJoint = GetComponent<HingeJoint2D>();
                if (nextJoint)
                {
                    Destroy(nextJoint);
                }
                nextRope.DetachAll();
            }

            SingletonMaster.Instance.m_playerControl.m_linkedObjects.Remove(nextObject);
            m_next.RemoveAt(i);
        }
    }
    
}
