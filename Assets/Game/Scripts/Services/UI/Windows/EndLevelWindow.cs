using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EndLevelWindow : BaseWindow
{
    [SerializeField] private float showButtonsDelay = .8f;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button completeButton;
    private Action _onRestartRequested;
    private Action _onGoNextLevelRequested;

    [Header("Animations")] 
    [SerializeField] private RectTransform buttons;
    [SerializeField] private float moveDuration = .2f;
    [SerializeField] private RectTransform animationStart;
    [SerializeField] private RectTransform animationEnd;

    public void SetUpCallbacks(Action onRestartRequested, Action onGoNextLevelRequested)
    {
        _onRestartRequested = onRestartRequested;
        _onGoNextLevelRequested = onGoNextLevelRequested;
    }

    public override void Show()
    {
        base.Show();
        restartButton.onClick.AddListener(HandleRestartClicked);
        completeButton.onClick.AddListener(HandleCompleteClicked);
        buttons.gameObject.SetActive(false);
        StartCoroutine(ShowButtons());
    }

    public override void Hide()
    {
        base.Hide();
        restartButton.onClick.RemoveAllListeners();
        completeButton.onClick.RemoveAllListeners();
    }

    private IEnumerator ShowButtons()
    {
        yield return new WaitForSeconds(showButtonsDelay);
        buttons.gameObject.SetActive(true);
        buttons.localPosition = animationStart.localPosition;
        buttons.DOLocalMove(animationEnd.localPosition, moveDuration)
            .SetLink(buttons.gameObject);
    }

    private void HandleRestartClicked()
    {
        _onRestartRequested?.Invoke();
    }
    
    private void HandleCompleteClicked()
    {
        _onGoNextLevelRequested?.Invoke();
    }
}