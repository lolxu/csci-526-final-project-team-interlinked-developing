using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeReceiver : MonoBehaviour
{
    public List<GameObject> m_links = new List<GameObject>();
    public int m_length = 15;
    
    public void ConnectToRope(Rigidbody2D ropeEnd)
    {
        HingeJoint2D joint = gameObject.AddComponent<HingeJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = ropeEnd;
        joint.anchor = Vector2.zero;
        joint.connectedAnchor = Vector2.zero;

        GetComponent<Rigidbody2D>().totalForce = Vector2.zero;
    }

    public void DestroyRope()
    {
        for (int i = m_links.Count - 1; i >= 0; i--)
        {
            GameObject curObj = m_links[i];
            m_links.RemoveAt(i);
            Destroy(curObj);
        }
        
        m_links.Clear();
    }
}
