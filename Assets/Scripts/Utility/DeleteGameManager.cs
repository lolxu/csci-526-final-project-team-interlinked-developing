using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteGameManager : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }
    }
}
