using System;
using YG;

public interface IPlatformSDK
{
    event Action OnAdShown;
    event Action OnAdClosed;
    
    SavesData CurrentData { get; }
    void ShowInterstitial();
    void ShowReward(string id, Action callBack);
    
    void Save();
    void WriteData(SavesData data);

    void GameReady();
    
    string Language { get; }
    
    bool IsFirstGameSession { get; }
}

[Serializable]
public class SavesData
{
    public int levelId;
    public bool[] openedLevels;
    public bool[] completedLevels;
}