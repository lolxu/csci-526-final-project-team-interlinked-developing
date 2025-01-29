using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class ShootComponent : MonoBehaviour
{
    [Header("Fire Projectile Settings")] 
    public GameObject m_bulletPrefab;
    public int m_fireRateChange = 0;
    public float m_recoilChange = 0.0f;
    public int m_penetrationChange = 0;
    
    [Header("Visual Settings")]
    [SerializeField] private GameObject m_gun;
    [SerializeField] private Light2D m_muzzleFlash;
    [SerializeField] private float m_muzzleFlashIntensity = 5.0f;
    
    private Rigidbody2D m_RB;
    private float m_fireTimeout = 0.0f;
    private bool m_isMouseDown;
    
    // Start is called before the first frame update
    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector3 selfToMouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.value) - transform.position;
        float angle = Mathf.Atan2(selfToMouse.y, selfToMouse.x) * Mathf.Rad2Deg;
        m_gun.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
    }

    // Update is called once per frame
    private void Update()
    {
        Vector2 myPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);

        // SHOOTING!!
        if (m_isMouseDown && m_bulletPrefab != null)
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
}
