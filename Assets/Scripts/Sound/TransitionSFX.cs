using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionSFX : MonoBehaviour
{
    [SerializeField] private float m_lifeTime = 1.0f;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        m_lifeTime -= Time.deltaTime;
        if (m_lifeTime <= 0.0f)
        {
            Destroy(gameObject);
        }
    }
}
