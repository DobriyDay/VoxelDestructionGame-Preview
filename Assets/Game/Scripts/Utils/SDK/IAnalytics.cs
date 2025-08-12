using YG;

public interface IAnalytics
{
    void LevelCompleted(int id);
    void LevelStarted(int id);
    void GameStarted();
    void Reward(string id);
}