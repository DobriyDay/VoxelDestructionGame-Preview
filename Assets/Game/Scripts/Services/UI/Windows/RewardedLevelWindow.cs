using System;
using UnityEngine;
using UnityEngine.UI;

public class RewardedLevelWindow : BaseWindow
{
    private Action _onPlay;
    private Action _onSkip;

    [SerializeField] private Button playButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private float skipDelay = 3f;

    private float _timer;
    private bool _skipAvailable;

    public void SetUpCallback(Action onPlay, Action onSkip)
    {
        _onPlay = onPlay;
        _onSkip = onSkip;
    }

    public override void Show()
    {
        base.Show();
        playButton.onClick.AddListener(PlayLevel);
        skipButton.onClick.AddListener(SkipLevel);
        
        skipButton.gameObject.SetActive(false);
        _skipAvailable = false;
        _timer = skipDelay;
    }

    public override void Hide()
    {
        base.Hide();
        playButton.onClick.RemoveAllListeners();
        skipButton.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (_skipAvailable) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _skipAvailable = true;
            skipButton.gameObject.SetActive(true);
        }
    }

    private void PlayLevel()
    {
        _onPlay?.Invoke();
    }

    private void SkipLevel()
    {
        _onSkip?.Invoke();
    } 
}