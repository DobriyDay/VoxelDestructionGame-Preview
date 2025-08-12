using System;
using DG.Tweening;
using UnityEngine;

public class Curtain : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject curtain;
    [SerializeField] private Transform background;
    [SerializeField] private Transform text;

    [Header("Animation")]
    [SerializeField] private float duration = 0.5f;

    [SerializeField] private Vector3 scaleOutPumped = new Vector3(1.2f, 1.2f, 1);
    [SerializeField] private Vector3 scaleShown = Vector3.one;
    [SerializeField] private Vector3 backgroundShown = Vector3.zero;

    private Tween _bgTween;
    private Sequence _textSequence;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitHiddenState();
    }

    private void InitHiddenState()
    {
        background.localScale = Vector3.zero;
        text.localScale = Vector3.zero;
    }

    public void ShowCurtain(Action onShown, bool instant = false)
    {
        curtain.SetActive(true);
        KillTweens();

        if (instant)
        {
            background.localScale = backgroundShown;
            text.localScale = scaleShown;
            onShown?.Invoke();
            return;
        }
        _bgTween = background.DOScale(backgroundShown, duration)
            .SetLink(background.gameObject);

        _textSequence = DOTween.Sequence()
            .Append(text.DOScale(scaleOutPumped, duration * 0.6f).SetEase(Ease.OutBack).SetLink(text.gameObject))
            .Append(text.DOScale(scaleShown, duration * 0.4f).SetEase(Ease.InOutSine).SetLink(text.gameObject))
            .OnComplete(() => onShown?.Invoke());
    }

    public void HideCurtain(Action onHide)
    {
        KillTweens();

        _bgTween = background.DOScale(Vector3.zero, duration);

        _textSequence = DOTween.Sequence()
            .Append(text.DOScale(scaleOutPumped, duration * 0.6f).SetEase(Ease.OutBack))
            .Append(text.DOScale(Vector3.zero, duration * .4f).SetEase(Ease.InBack))
            .OnComplete(() => OnHidden(onHide));
    }

    private void OnHidden(Action onHide)
    {
        curtain.SetActive(false);
        onHide?.Invoke();
    }

    private void KillTweens()
    {
        _bgTween?.Kill();
        _textSequence?.Kill();
    }
}