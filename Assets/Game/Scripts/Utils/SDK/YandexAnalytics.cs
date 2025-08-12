using UnityEngine;
using YG;

public class YandexAnalytics : IAnalytics
{
    public void LevelCompleted(int id)
    {
        YG2.MetricaSend($"level_complete_{id}");
        Debug.Log("LevelCompleted");
    }

    public void LevelStarted(int id)
    {
        YG2.MetricaSend($"level_start_{id}");
        Debug.Log("LevelStarted");
    }

    public void GameStarted()
    {
        if (Global.PlatformSDK.IsFirstGameSession)
        {
            YG2.MetricaSend("game_started");
        }
    }

    public void Reward(string id)
    {
        YG2.MetricaSend(id);
    }
}