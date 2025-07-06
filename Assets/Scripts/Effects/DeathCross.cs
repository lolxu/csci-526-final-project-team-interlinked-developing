using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class DeathCross : MonoBehaviour
{
    [SerializeField] private float m_animTime = 0.25f;
    [SerializeField] private float m_scale = 1.0f;
    [SerializeField] private Ease m_showEase;
    [SerializeField] private Ease m_disappearEase;
    [SerializeField] private SpriteRenderer m_cross_A;
    [SerializeField] private SpriteRenderer m_cross_B;

    public AudioClip castSound; // Free's sound

    private Vector3 m_finalScale;

    private void OnEnable()
    {
        AudioManager.Instance.PlayOtherSFX(castSound); // Free's sound

        m_finalScale = new Vector3(m_scale, m_scale, m_scale);
        transform.localScale = Vector3.zero;

        m_cross_A.DOColor(Color.white, 0.15f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutFlash);
        m_cross_B.DOColor(Color.white, 0.15f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutFlash);
        
        transform.DOScale(m_finalScale, m_animTime).SetEase(m_showEase).OnComplete(() =>
        {
            transform.DOScale(Vector3.zero, m_animTime).SetEase(m_disappearEase).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        });
    }
}
