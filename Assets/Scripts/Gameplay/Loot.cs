using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    [SerializeField] private float m_shrinkSpeed = 1.25f;
    private Coroutine shrinkCoroutine = null;
    
    public void StartShrinking()
    {
        if (shrinkCoroutine == null)
        {
            shrinkCoroutine = StartCoroutine(ShrinkSequence());
        }
    }

    private IEnumerator ShrinkSequence()
    {
        while (transform.localScale.x >= 0.0f)
        {
            transform.localScale -= Vector3.one * m_shrinkSpeed * Time.fixedDeltaTime;
            yield return null;
        }
        SingletonMaster.Instance.EventManager.LootCollected.Invoke();
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}
