using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelsLoader : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private LevelsList levelsList;
    
    [Header("Debug")] 
    [SerializeField] private int debugLevelID = -1;
      
    private readonly Dictionary<int, LevelReferenceById> _levelsByID = new ();
    private IAssetProvider _assetProvider;
    private IPlatformSDK _platformSDK;
    private LevelReferenceById _currentLevelReference;
    
    public LevelReferenceById CurrentLevelReference => _currentLevelReference;

    public void Initialize(IAssetProvider assetProvider)
    {
        _assetProvider = assetProvider;
        _platformSDK = Global.PlatformSDK;
        for (int i = 0; i < levelsList.levelsByID.Length; i++)
        {
            _levelsByID.Add(levelsList.levelsByID[i].id, levelsList.levelsByID[i]);
        }
    }

    public async UniTask<LevelDataFile> LoadLevel()
    {
        if (debugLevelID != -1)
        {
            Debug.Log("<color=yellow>DEBUG LEVEL</color>");
            return await LoadLevelFile(debugLevelID);
        }
        
        int levelID = _platformSDK.CurrentData.levelId;
        return await LoadLevelFile(levelID);
    }

    private async UniTask<LevelDataFile> LoadLevelFile(int id)
    {
        _currentLevelReference = _levelsByID[id];
        return await _assetProvider.LoadAsync<LevelDataFile>(_currentLevelReference.levelDataPath);
    }

    public void CompleteCurrentLevel()
    {
        for (int i = 0; i < levelsList.levelsByID.Length; i++)
        {
            if (levelsList.levelsByID[i].id == _currentLevelReference.id)
            {
                int nextLevelID = 0;
                if (i + 1 < levelsList.levelsByID.Length)
                {
                    nextLevelID = levelsList.levelsByID[i + 1].id;
                }

                var currentData = Global.PlatformSDK.CurrentData;
                currentData.levelId = nextLevelID;
                Global.PlatformSDK.WriteData(currentData);
                
                break;
            }
        }
    }

    public void UnloadCurrentLevel()
    {
        if (_currentLevelReference != null)
            _assetProvider.Unload(_currentLevelReference.levelDataPath);
    }
}