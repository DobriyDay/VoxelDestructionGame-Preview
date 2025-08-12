using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameplayFlow
{
        private readonly PlayerChunks _player;
        private readonly CameraContainer _camera;
        private readonly LevelBuilder _levelBuilder;
        private readonly Curtain _curtain;
        private readonly UIWindowManager _uiWindowManager;
        private readonly DragController _dragController;
        private readonly LevelsLoader _levelsLoader;

        public GameplayFlow(PlayerChunks player, CameraContainer camera, LevelsLoader levelsLoader, LevelBuilder levelBuilder,
                Curtain curtain, UIWindowManager uiWindowManager, DragController dragController)
        {
                _player = player;
                _camera = camera;
                _levelBuilder = levelBuilder;
                _curtain = curtain;
                _uiWindowManager = uiWindowManager;
                _dragController = dragController;
                _levelsLoader = levelsLoader;
                
                SetUpWindowsCallbacks();
        }

        private void SetUpWindowsCallbacks()
        {
                var gameWindow = _uiWindowManager.Get<GameplayWindow>();
                gameWindow.SetUpCallbacks(RestartLevel, () => _uiWindowManager.Show<SettingsWindow>(true));
                
                var settingsWindow = _uiWindowManager.Get<SettingsWindow>();
                settingsWindow.SetUpCallback(ShowGameplayWindow);
                
                var rewardedLevel = _uiWindowManager.Get<RewardedLevelWindow>();
                rewardedLevel.SetUpCallback(ShowRewardedLevelAd, SkipRewardedLevel);
                
                var endLevelWindow = _uiWindowManager.Get<EndLevelWindow>();
                endLevelWindow.SetUpCallbacks(ShowRewardedAndRestart, CompleteLevelAndShowAd);
        }

        public async UniTaskVoid LoadCurrentLevel()
        {
                var levelDataFile = await _levelsLoader.LoadLevel();
                await _levelBuilder.BuildLevelFromFile(levelDataFile);
                
                _camera.Follow(_player.CameraFollowPoint);

                if (!levelDataFile.IsRewardedLevel)
                        _curtain.HideCurtain(StartGame().Forget);
                else
                {
                        _curtain.HideCurtain(SubscribeToRewardedLevelWindow);
                }
        }

        private void SubscribeToRewardedLevelWindow()
        {
                if (Global.PlatformSDK.CurrentData.openedLevels[_levelsLoader.CurrentLevelReference.id])
                {
                        StartGame().Forget();
                        return;
                }
                
                _uiWindowManager.Show<RewardedLevelWindow>(true);
        }
        
        private void ShowRewardedLevelAd()
        {
                Global.PlatformSDK.ShowReward($"RewardedLevel_{_levelsLoader.CurrentLevelReference.id}", StartGame().Forget);
        }
        
        private void SkipRewardedLevel()
        {
                _uiWindowManager.Hide<RewardedLevelWindow>();
                SaveCurrentLevelCompletion();
                CompleteLevel();
        }

        private void RestartLevel()
        {
                _levelBuilder.RestartLevel();
                StartGame().Forget();
        }

        private void CompleteLevel()
        {
                _uiWindowManager.Hide<EndLevelWindow>();
                _levelBuilder.UnloadLevelData();
                _levelsLoader.UnloadCurrentLevel();
                _curtain.ShowCurtain(LoadCurrentLevel().Forget);
        }

        private void SaveCurrentLevelCompletion()
        {
                _levelsLoader.CompleteCurrentLevel();
                Global.PlatformSDK.Save();
        }
      
        private void OnCompleteLevel()
        {
                CloseLevel();
                _dragController.RemoveCurrentTarget();
                SaveCurrentLevelCompletion();
                _uiWindowManager.Show<EndLevelWindow>(true);
        }
        
        private void ShowRewardedAndRestart()
        {
                Global.PlatformSDK.ShowReward($"RestartLevel_{_levelsLoader.CurrentLevelReference.id}", RestartLevel);
        }

        private void CompleteLevelAndShowAd()
        {
                Global.PlatformSDK.ShowInterstitial();
                CompleteLevel();
        }
        
        private async UniTaskVoid StartGame()
        {
                await UniTask.DelayFrame(1);
                OpenLevel();
                ShowGameplayWindow();

                _camera.Follow(_player.CameraFollowPoint);
                
                _levelBuilder.ActivateLevelObjects();
                _player.OnDestroyed += StopGame;
                Global.PlatformSDK.GameReady();
        }

        private void OpenLevel()
        {
                if (!Global.PlatformSDK.CurrentData.openedLevels[_levelsLoader.CurrentLevelReference.id])
                {
                        Global.PlatformSDK.CurrentData.openedLevels[_levelsLoader.CurrentLevelReference.id] = true;
                        Global.Analytics.LevelStarted(_levelsLoader.CurrentLevelReference.id);
                        Global.PlatformSDK.Save();
                }
        }
        
        private void CloseLevel()
        {
                if (!Global.PlatformSDK.CurrentData.completedLevels[_levelsLoader.CurrentLevelReference.id])
                {
                        Global.PlatformSDK.CurrentData.completedLevels[_levelsLoader.CurrentLevelReference.id] = true;
                        Global.Analytics.LevelCompleted(_levelsLoader.CurrentLevelReference.id);
                        Global.PlatformSDK.Save();
                }
        }
        
        private void ShowGameplayWindow()
        {
                _dragController.CanDrag = true;
                _uiWindowManager.Show<GameplayWindow>(true);  
        }

        private void StopGame()
        {
                _dragController.CanDrag = false;
                _player.OnDestroyed -= StopGame;
                _camera.Follow(null);
                
                OnCompleteLevel();
        }
}