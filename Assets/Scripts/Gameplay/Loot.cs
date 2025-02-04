using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    [SerializeField] private float m_shrinkSpeed = 1.25f;
    
    public void StartShrinking()
    {
        SingletonMaster.Instance.EventManager.LootCollected.Invoke();
        StartCoroutine(ShrinkSequence());
    }

    private IEnumerator ShrinkSequence()
    {
        while (transform.localScale.x >= 0.0f)
        {
            transform.localScale -= Vector3.one * m_shrinkSpeed * Time.fixedDeltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }
}
