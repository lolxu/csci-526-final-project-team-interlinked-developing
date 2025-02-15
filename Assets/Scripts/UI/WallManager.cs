using System.Collections;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public Transform boundaryU; // Top Wall
    public Transform boundaryD; // Bottom Wall
    public Transform boundaryL; // Left Wall
    public Transform boundaryR; // Right Wall

    public float shrinkSpeed = 1.0f; // Speed at which walls shrink
    public float minArenaSize = 4.0f; // Minimum width & height before stopping
    public float maxShrinkDistance = 10.0f; // Maximum allowed shrink distance
    private bool shrinking = false;
    private UI uiManager;

    private Vector3 originalPosU, originalPosD, originalPosL, originalPosR; // Store original positions

    private void Start()
    {
        // Find the UI Manager in the scene
        uiManager = FindObjectOfType<UI>();

        // Find boundaries automatically if tags are used
        if (boundaryU == null) boundaryU = GameObject.FindWithTag("TopWall")?.transform;
        if (boundaryD == null) boundaryD = GameObject.FindWithTag("BottomWall")?.transform;
        if (boundaryL == null) boundaryL = GameObject.FindWithTag("LeftWall")?.transform;
        if (boundaryR == null) boundaryR = GameObject.FindWithTag("RightWall")?.transform;

        // Store original positions for resetting later
        originalPosU = boundaryU.position;
        originalPosD = boundaryD.position;
        originalPosL = boundaryL.position;
        originalPosR = boundaryR.position;

        SingletonMaster.Instance.EventManager.WaveTimeoutEvent.AddListener(ShrinkWalls);
        SingletonMaster.Instance.EventManager.NextWaveEvent.AddListener(ResetWalls); // Fixed method signature
    }

    private void ShrinkWalls()
    {
        if (!shrinking)
        {
            shrinking = true;
            if (uiManager != null)
            {
                uiManager.EnqueueAnnouncement("The walls are shrinking!", 2.0f);
            }
            StartCoroutine(ShrinkWallsRoutine());
        }
    }

    private IEnumerator ShrinkWallsRoutine()
    {
        while (shrinking)
        {
            float currentWidth = Vector3.Distance(boundaryL.position, boundaryR.position);
            float currentHeight = Vector3.Distance(boundaryU.position, boundaryD.position);

            float movedU = Vector3.Distance(boundaryU.position, originalPosU);
            float movedD = Vector3.Distance(boundaryD.position, originalPosD);
            float movedL = Vector3.Distance(boundaryL.position, originalPosL);
            float movedR = Vector3.Distance(boundaryR.position, originalPosR);

            // Stop shrinking if walls have moved too far or reached minimum size
            if (currentWidth <= minArenaSize || currentHeight <= minArenaSize ||
                movedU >= maxShrinkDistance || movedD >= maxShrinkDistance ||
                movedL >= maxShrinkDistance || movedR >= maxShrinkDistance)
            {
                shrinking = false;
                if (uiManager != null)
                {
                    uiManager.EnqueueAnnouncement("Walls stopped shrinking!", 2.0f);
                }
                yield break; // Stop coroutine
            }

            // Move walls inward
            boundaryL.position += new Vector3(shrinkSpeed * Time.deltaTime, 0, 0);
            boundaryR.position -= new Vector3(shrinkSpeed * Time.deltaTime, 0, 0);
            boundaryU.position -= new Vector3(0, shrinkSpeed * Time.deltaTime, 0);
            boundaryD.position += new Vector3(0, shrinkSpeed * Time.deltaTime, 0);

            yield return null;
        }
    }

    private void ResetWalls(EnemySpawnScriptable wave)
    {
        Debug.Log("[WallManager] Resetting Walls to Original Position...");
        boundaryU.position = originalPosU;
        boundaryD.position = originalPosD;
        boundaryL.position = originalPosL;
        boundaryR.position = originalPosR;
        shrinking = false;
    }

    private void OnDestroy()
    {
        SingletonMaster.Instance.EventManager.WaveTimeoutEvent.RemoveListener(ShrinkWalls);
        SingletonMaster.Instance.EventManager.NextWaveEvent.RemoveListener(ResetWalls);
    }
}
