using System;
using UnityEngine;
using UnityEngine.UI;

public class GameplayWindow : BaseWindow
{
        private Action _onRestart;
        private Action _onSettings;
        
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;

        public void SetUpCallbacks(Action onRestart, Action onSettings)
        {
                _onRestart = onRestart;
                _onSettings = onSettings;
        }
        
        private void OnEnable()
        {
                restartButton.onClick.AddListener(RestartLevel);
                settingsButton.onClick.AddListener(OpenSettings);
        }

        private void OnDisable()
        {
                restartButton.onClick.RemoveAllListeners();
                settingsButton.onClick.RemoveAllListeners();
        }

        private void RestartLevel()
        {
                _onRestart?.Invoke();  
        }
        
        private void OpenSettings()
        {
                _onSettings?.Invoke();
        }
}