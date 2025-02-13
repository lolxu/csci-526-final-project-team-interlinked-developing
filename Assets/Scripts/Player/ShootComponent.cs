using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ShootComponent : MonoBehaviour
{
    public enum GunType
    {
        Pistol,
        Sniper,
        Shotgun
    }

    [Header("Fire Projectile Settings")] 
    public GunType m_type;
    public GameObject m_playerBulletPrefab;
    public GameObject m_enemyBulletPrefab;
    public int m_fireRateChange = 0;
    public float m_recoilChange = 0.0f;
    public int m_penetrationChange = 0;
    public bool m_canShoot = false;
    public bool m_canAutoAim = false;

    [Header("Ability Settings")] 
    [SerializeField] private AbilityScriptable m_autoAimAbility;
    public float m_autoAimRadius = 10.0f;
    
    [Header("Visual Settings")]
    [SerializeField] private GameObject m_gun;
    [SerializeField] private GameObject m_muzzle;
    [SerializeField] private LineRenderer m_laser;
    [SerializeField] private Light2D m_muzzleFlash;
    [SerializeField] private float m_muzzleFlashIntensity = 5.0f;
    [SerializeField] private Gradient m_enemySniperTelegraph;

    [Header("Durability")] 
    public DurabilityComponent m_durabilityComponent;
    
    private Rigidbody2D m_RB;
    private float m_fireTimeout = 0.0f;
    private bool m_isMouseDown = false;
    private bool m_isOwnerEnemy = false;
    private Vector2 m_aimOffset = Vector2.zero;
    private bool m_hasTarget = false;
    private bool m_canTurnToPlayer = true;

    private Coroutine m_enemyFireRoutine = null;
    
    // Start is called before the first frame update
    private void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
        
        SingletonMaster.Instance.EventManager.StartFireEvent.AddListener(StartFiring);
        SingletonMaster.Instance.EventManager.StopFireEvent.AddListener(StopFiring);
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.AddListener(OnPlayerDeath);
        
        SingletonMaster.Instance.EventManager.LinkEvent.AddListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.AddListener(OnUnlinked);
    }

    private void OnPlayerDeath(GameObject obj)
    {
        m_canShoot = false;
    }

    private void OnUnlinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            if (instigator.CompareTag("Player"))
            {
                m_canShoot = false;
                
                Debug.Log(instigator + " " + m_isOwnerEnemy);

                if (m_isOwnerEnemy && m_enemyFireRoutine == null)
                {
                    if (m_type == GunType.Pistol)
                    {
                        m_canTurnToPlayer = true;
                        m_enemyFireRoutine = StartCoroutine(EnemyPistolShoot());
                    }
                    else if (m_type == GunType.Sniper)
                    {
                        m_canTurnToPlayer = true;
                        m_enemyFireRoutine = StartCoroutine(EnemySniperShoot());
                    }
                }
            }
            
            if (instigator.CompareTag("Enemy"))
            {
                m_isOwnerEnemy = false;

                if (m_enemyFireRoutine != null)
                {
                    StopCoroutine(m_enemyFireRoutine);
                    m_enemyFireRoutine = null;
                    m_canTurnToPlayer = false;
                    m_fireTimeout = 0.0f;
                }
            }
        }
    }

    private void OnLinked(GameObject obj, GameObject instigator)
    {
        if (obj == gameObject)
        {
            m_canShoot = instigator.CompareTag("Player");

            if (!m_isOwnerEnemy && instigator.CompareTag("Enemy"))
            {
                m_isOwnerEnemy = true;
            }

            if (m_canShoot)
            {
                if (m_enemyFireRoutine != null)
                {
                    StopCoroutine(m_enemyFireRoutine);
                    m_enemyFireRoutine = null;
                    m_canTurnToPlayer = false;
                    m_fireTimeout = 0.0f;
                }
            }
            else if (m_isOwnerEnemy && m_enemyFireRoutine == null)
            {
                if (m_type == GunType.Pistol)
                {
                    m_canTurnToPlayer = true;
                    m_enemyFireRoutine = StartCoroutine(EnemyPistolShoot());
                }
                else if (m_type == GunType.Sniper)
                {
                    m_canTurnToPlayer = true;
                    m_enemyFireRoutine = StartCoroutine(EnemySniperShoot());
                }
            }
        }
    }

    private void OnDisable()
    {
        SingletonMaster.Instance.EventManager.StartFireEvent.RemoveListener(StartFiring);
        SingletonMaster.Instance.EventManager.StopFireEvent.RemoveListener(StopFiring);
        SingletonMaster.Instance.EventManager.PlayerDeathEvent.RemoveListener(OnPlayerDeath);
        
        SingletonMaster.Instance.EventManager.LinkEvent.RemoveListener(OnLinked);
        SingletonMaster.Instance.EventManager.UnlinkEvent.RemoveListener(OnUnlinked);
    }

    private void FixedUpdate()
    {
        if (m_canShoot)
        {
            Vector3 selfToTarget = Camera.main.ScreenToWorldPoint(Mouse.current.position.value) - transform.position;

            if (m_canAutoAim)
            {
                selfToTarget = AutoAim(selfToTarget, LayerMask.GetMask("Enemy"));
            }
            
            // Rotating the gun
            float angle = Mathf.Atan2(selfToTarget.y, selfToTarget.x) * Mathf.Rad2Deg;
            m_gun.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
        }
        else if (m_isOwnerEnemy)
        {
            Vector3 selfToTarget = Vector3.zero;
            selfToTarget = AutoAim(selfToTarget, LayerMask.GetMask("Player"));
            
            // Rotating the gun
            if (m_hasTarget && m_canTurnToPlayer)
            {
                float angle = Mathf.Atan2(selfToTarget.y, selfToTarget.x) * Mathf.Rad2Deg;
                m_gun.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        m_canAutoAim = m_autoAimAbility.m_enabled && m_canShoot;
        
        Vector2 myPos = transform.position;
        Vector2 muzzlePosition = m_muzzle.transform.position;
        
        if (m_canShoot)
        {
            // SHOOTING!!
            if (m_isMouseDown && m_playerBulletPrefab != null && m_durabilityComponent.m_currentDurability > 0)
            {
                // Here's the shooty controls
                m_fireTimeout -= Time.deltaTime;
                if (m_fireTimeout <= 0.0f)
                {
                    ShootBullet(muzzlePosition, myPos, m_playerBulletPrefab);
                    m_durabilityComponent.UseDurability();
                }
            }
        }
        
        m_muzzleFlash.intensity -= 50.0f * Time.deltaTime;
        if (m_muzzleFlash.intensity <= 0.0f)
        {
            m_muzzleFlash.intensity = 0.0f;
        }
    }

    private IEnumerator EnemyPistolShoot()
    {
        while (m_isOwnerEnemy)
        {
            yield return new WaitForSeconds(m_fireTimeout);
            if (m_hasTarget)
            {
                Vector2 myPos = transform.position;
                Vector2 muzzlePosition = m_muzzle.transform.position;
                ShootBullet(muzzlePosition, myPos, m_enemyBulletPrefab);
            }
        }
    }

    private IEnumerator EnemySniperShoot()
    {
        if (m_laser != null)
        {
            while (m_isOwnerEnemy)
            {
                yield return new WaitForSeconds(m_fireTimeout);
                if (m_hasTarget)
                {
                    Vector2 myPos = transform.position;
                    Vector2 muzzlePosition = m_muzzle.transform.position;
                    m_canTurnToPlayer = false;
                    Gradient oldGrad = m_laser.colorGradient;
                    yield return new WaitForSeconds(0.15f);
                    m_laser.colorGradient = m_enemySniperTelegraph;
                    yield return new WaitForSeconds(0.15f);
                    m_laser.colorGradient = oldGrad;
                    yield return new WaitForSeconds(0.15f);
                    m_laser.colorGradient = m_enemySniperTelegraph;
                    yield return new WaitForSeconds(0.15f);
                    m_laser.colorGradient = oldGrad;
                    ShootBullet(muzzlePosition, myPos, m_enemyBulletPrefab);
                    m_canTurnToPlayer = true;
                }
            }
        }
    }

    private Vector3 AutoAim(Vector3 selfToTarget, int layer)
    {
        RaycastHit2D[] results = new RaycastHit2D[50];
        var size = Physics2D.CircleCastNonAlloc(transform.position, m_autoAimRadius, Vector2.zero, results, 0.0f, layer);
        float minDist = float.MaxValue;
        GameObject bestTarget = null;
        for (int i = 0; i < size; i++)
        {
            RaycastHit2D enemy = results[i];
            float dist = Vector2.Distance(enemy.transform.position, transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                bestTarget = enemy.collider.gameObject;
            }
        }
            
        // Debug.Log(bestTarget);

        if (bestTarget != null)
        {
            selfToTarget = bestTarget.transform.position - transform.position;
            m_hasTarget = true;
        }
        else
        {
            m_hasTarget = false;
        }

        return selfToTarget;
    }

    private void ShootBullet(Vector3 muzzlePosition, Vector3 myPos, GameObject bulletPrefab)
    {
        m_muzzleFlash.intensity = m_muzzleFlashIntensity;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        BasePlayerBullet bulletScript = bullet.GetComponent<BasePlayerBullet>();
        bulletScript.m_owner = gameObject;
        
        //TODO: Make sure things don't go negative...
        // Modding stats for bullets
        bulletScript.m_penetrateNum += m_penetrationChange;
        m_fireTimeout = 60.0f / (bulletScript.m_fireRate + m_fireRateChange);
        // bulletScript.m_direction = (mouseWorldPos - myPos).normalized;
        bulletScript.m_direction = (muzzlePosition - myPos).normalized;

        // Add some recoil;
        m_RB.AddForce(-bulletScript.m_direction * (bulletScript.m_recoil + m_recoilChange),
            ForceMode2D.Impulse);
    }

    private void StartFiring()
    {
        m_isMouseDown = true;
    }

    private void StopFiring()
    {
        m_isMouseDown = false;
    }
}
