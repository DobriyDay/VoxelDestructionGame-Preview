using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : BaseWindow
{
    private Action _onResume;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Image audioButton;
    [SerializeField] private Sprite audioOnSprite;
    [SerializeField] private Sprite audioOffSprite;
    private bool _audioOn = true;
        
    public void SetUpCallback(Action onResume)
        => _onResume = onResume;

    public override void Show()
    {
        base.Show();
        SetAudio(0.3f);
        GamePause.SetPause(PauseSource.Game, true, false);
        resumeButton.onClick.AddListener(Hide);
    }

    public override void Hide()
    {
        base.Hide();
        SetAudio(1);
        GamePause.SetPause(PauseSource.Game, false, false);
        resumeButton.onClick.RemoveAllListeners();
        _onResume?.Invoke();
    }
    
    private void SetAudio(float maxAudio)
    {
        audioButton.sprite = _audioOn ? audioOnSprite : audioOffSprite;
        AudioListener.volume = _audioOn ? maxAudio : 0;
    }
    
    public void SwitchAudio()
    {
        _audioOn = !_audioOn;
        SetAudio(0.3f);
    }
}