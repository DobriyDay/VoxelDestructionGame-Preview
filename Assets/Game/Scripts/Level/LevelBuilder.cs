using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public class LevelBuilder
{
    private struct ControllableDefaults
    {
        public bool isStatic;
        public bool isControllable;
        public bool isStaticUntilTouch;
    }
    
    private readonly PlayerChunks _player;
    private readonly IAssetProvider _assetProvider;
    private readonly Transform[] _levelBounds;
    private readonly EnvironmentStyler _environmentStyler;
    private LevelDataFile _levelDataFile;
    private readonly Dictionary<GameObject, TransformCoordinates> _spawnedObjects = new ();
    private readonly List<IInitializableSpawnableObject> _spawnedList = new ();
    private readonly Dictionary<IControllableObject, ControllableDefaults> _controllableDefaults = new ();
    
    public LevelBuilder(PlayerChunks player, IAssetProvider assetProvider, Transform[] levelBounds, EnvironmentStyler environmentStyler)
    {
        _player = player;
        _assetProvider = assetProvider;
        _levelBounds = levelBounds;
        _environmentStyler = environmentStyler;

        for (int i = 0; i < _levelBounds.Length; i++)
        {
            _levelBounds[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }
    
    public async UniTask BuildLevelFromFile(LevelDataFile file)
    {
        _controllableDefaults.Clear();
        _levelDataFile = file;
        for (int i = 0; i < _levelDataFile.levelObjects.Length; i++)
        {
            var objectInstance = await _assetProvider.LoadAsync<GameObject>(_levelDataFile.levelObjects[i].prefabReference);
            var coords = _levelDataFile.levelObjects[i].coordinates;
            for (int j = 0; j < coords.Length; j++)
            {
                var spawnedObject = Object.Instantiate(objectInstance, coords[j].position, coords[j].rotation);
                _spawnedObjects.Add(spawnedObject, coords[j]);
                if (spawnedObject.TryGetComponent(out IInitializableSpawnableObject initObj))
                {
                    _spawnedList.Add(initObj);
                    initObj.Initialize();
                }

                if (spawnedObject.TryGetComponent(out IControllableObject controllableObj))
                {
                    controllableObj.SetUp(
                        _levelDataFile.levelObjects[i].isControllable[j],
                        _levelDataFile.levelObjects[i].isStatic[j],
                        _levelDataFile.levelObjects[i].isStaticUntilTouch[j]
                        );
                    _controllableDefaults[controllableObj] = new ControllableDefaults
                    {
                        isStatic = _levelDataFile.levelObjects[i].isStatic[j],
                        isControllable = _levelDataFile.levelObjects[i].isControllable[j],
                        isStaticUntilTouch = _levelDataFile.levelObjects[i].isStaticUntilTouch[j]
                    };
                }
            }
        }
        
        var playerPosition = _levelDataFile.playerStartPosition;
        _player.Initialize();
        if (_levelDataFile.handsItems != null && _levelDataFile.handsItems.Length > 0)
        {
            _player.HandsItems[0].ChooseItem(_levelDataFile.handsItems[0]);
            _player.HandsItems[1].ChooseItem(_levelDataFile.handsItems[1]);
        }

        _player.transform.position = playerPosition;

        for (int i = 0; i < _levelDataFile.bounds.Length; i++)
        {
            _levelBounds[i].position = _levelDataFile.bounds[i].position;
            _levelBounds[i].rotation = _levelDataFile.bounds[i].rotation;
            _levelBounds[i].localScale = _levelDataFile.bounds[i].scale;
        }
        
        _environmentStyler.SetStyle(_levelDataFile.environmentStyle);
        
        await UniTask.WaitForSeconds(0.5f);
    }

    public void RestartLevel()
    {
        if (_levelDataFile == null)
            return;
        
        _player.Reset();
        _player.Initialize();

        foreach (var _ctrDefaults in _controllableDefaults)
        {
            _ctrDefaults.Key.SetUp(_ctrDefaults.Value.isControllable, _ctrDefaults.Value.isStatic, _ctrDefaults.Value.isStaticUntilTouch);
        }
        
        for (int i = 0; i < _spawnedList.Count; i++)
        {
            _spawnedList[i].Reset();
            _spawnedList[i].Initialize();
        }
        
        foreach (var spawnedObjectPair in _spawnedObjects)
        {
            if (spawnedObjectPair.Key.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
            
            spawnedObjectPair.Key.transform.position = spawnedObjectPair.Value.position;
            spawnedObjectPair.Key.transform.rotation = spawnedObjectPair.Value.rotation;
            spawnedObjectPair.Key.transform.localScale = spawnedObjectPair.Value.scale;
        }
    }

    public void UnloadLevelData()
    {
        for (int i = 0; i < _spawnedList.Count; i++)
        {
            _spawnedList[i].Reset();
        }
        
        foreach (var object_ in _spawnedObjects.Keys)
        {
            Object.Destroy(object_);
        }

        _spawnedObjects.Clear();
        _spawnedList.Clear();

        for (int i = 0; i < _levelDataFile.levelObjects.Length; i++)
        {
            _assetProvider.Unload(_levelDataFile.levelObjects[i].prefabReference);
        }
        
        _player.Reset();
        _levelDataFile = null;
    }

    public void ActivateLevelObjects()
    {
        _player.Run();
        for (int i = 0; i < _spawnedList.Count; i++)
        {
            _spawnedList[i].Run();
        }
    }
}