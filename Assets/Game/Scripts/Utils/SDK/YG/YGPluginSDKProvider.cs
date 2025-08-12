using System;
using UnityEngine;
using YG;

public class YGPluginSDKProvider : IPlatformSDK
{
    public event Action OnAdShown;
    public event Action OnAdClosed;
    
    public SavesData CurrentData { get; private set; }
    public string Language => YG2.lang;
    public bool IsFirstGameSession => YG2.isFirstGameSession;
    
    private bool _gameReadyDone = false;

    public YGPluginSDKProvider()
    {
        YG2.onFocusWindowGame += OnFocusWindowGame;
        YG2.onOpenInterAdv += OnOpenAnyAdv;
        YG2.onCloseInterAdv += OnCloseAnyAdv;
        
        YG2.onOpenRewardedAdv += OnOpenAnyAdv;
        YG2.onCloseRewardedAdv += OnCloseAnyAdv;
        
        LoadSavesData();
        
        OnFocusWindowGame(YG2.isFocusWindowGame);
    }


    public void GameReady()
    {
        if (_gameReadyDone)
            return;
        
        _gameReadyDone = true;
        YG2.GameReadyAPI();
    }

    private void LoadSavesData()
    {
        CurrentData = new SavesData
        {
            levelId = YG2.saves.levelId,
            openedLevels = YG2.saves.openedLevels,
            completedLevels = YG2.saves.completedLevels,
        };
    }
    
    private void OnFocusWindowGame(bool focused)
    {
        if (!focused)
            GamePause.SetPause(PauseSource.Focus, true);
        else
            GamePause.SetPause(PauseSource.Focus, false);
    }

    private void OnCloseAnyAdv()
    {
        GamePause.SetPause(PauseSource.Ad, false);
        OnAdClosed?.Invoke();
    }

    private void OnOpenAnyAdv()
    {
        GamePause.SetPause(PauseSource.Ad, true);
        OnAdShown?.Invoke();
    }

    public void ShowInterstitial()
    {
        YG2.InterstitialAdvShow();
    }

    public void ShowReward(string id, Action callBack)
    {
        YG2.RewardedAdvShow(id, () => RewardedWithCallback(id, callBack));
    }

    private void RewardedWithCallback(string id, Action callBack)
    {
        Global.Analytics.Reward(id);
        callBack?.Invoke();
    }
    
    public void Save()
    {
        YG2.SaveProgress();
    }

    public void WriteData(SavesData data)
    {
        CurrentData = data;
        YG2.saves.levelId = CurrentData.levelId;
        YG2.saves.openedLevels = CurrentData.openedLevels;
        YG2.saves.completedLevels = CurrentData.completedLevels;
    }
}
