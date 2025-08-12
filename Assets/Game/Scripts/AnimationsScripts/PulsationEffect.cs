using System;
using UnityEngine;
using DG.Tweening;

public class PulsationEffect : MonoBehaviour
{
    [SerializeField] private bool disableAfterPulse = false;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float cooldown = 0.1f;
    [SerializeField] private int loops = 1;
    [SerializeField] private bool startOnEnable = false;

    private Tween _pulseTween;
    private Vector3 _originalScale;
    private float _lastPulseTime;
    
    private void OnEnable()
    {
        if (startOnEnable)
            StartPulse();
    }

    public void StartPulse()
    {
        if (_originalScale == Vector3.zero)
            _originalScale = transform.localScale;
        
        gameObject.SetActive(true);
        
        if (Time.time - _lastPulseTime < cooldown || _pulseTween.IsActive() && _pulseTween.IsPlaying())
            return;

        StopPulse();

        _lastPulseTime = Time.time;
        _pulseTween = transform.DOScale(_originalScale * pulseScale, duration)
            .SetLoops(loops, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject)
            .OnComplete(OnPulseComplete);
    }

    private void OnPulseComplete()
    {
        transform.localScale = _originalScale;
        _pulseTween = null;

        if (disableAfterPulse)
            gameObject.SetActive(false);
    }

    public void StopPulse()
    {
        if (_pulseTween != null && _pulseTween.IsActive())
        {
            _pulseTween.Kill();
            transform.localScale = _originalScale;
            _pulseTween = null;
        }
    }
}