using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera m_camera;
    private Coroutine m_shakeCoroutine = null;
    
    public void Shake(float intensity, float duration)
    {
        if (m_shakeCoroutine != null)
        {
            StopCoroutine(m_shakeCoroutine);
        }

        m_shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        CinemachineBasicMultiChannelPerlin perlin =
            m_camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (perlin != null)
        {
            perlin.m_AmplitudeGain = intensity;
            while (duration >= 0.0f)
            {
                yield return null;
                duration -= Time.deltaTime;
            }
            perlin.m_AmplitudeGain = 0.0f;
        }
        
    }
}
