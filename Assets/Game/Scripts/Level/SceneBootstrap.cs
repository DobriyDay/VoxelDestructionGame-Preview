using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

public class SceneBootstrap : MonoBehaviour
{
      [SerializeField] private Transform bounds;
      [SerializeField] private EnvironmentStyler environmentStyler;
      [SerializeField] private CameraContainer cameraContainer;
      [SerializeField] private PlayerChunks player;
      [SerializeField] private LevelsLoader levelsLoader;
      [SerializeField] private UIWindowManager uiWindowManager;
      [SerializeField] private DragController dragController;
      
      [SerializeField] private Curtain curtain;
      
      private IAssetProvider _assetProvider;
      private LevelBuilder _levelBuilder;
      private GameplayFlow _gameplayFlow;
      
      private void Awake()
      {
            _assetProvider = new AddressablesAssetProvider();
            
            var bounds_ = new Transform[bounds.childCount];
            for (int i = 0; i < bounds_.Length; i++)
                  bounds_[i] = bounds.GetChild(i);
            
            _levelBuilder = new LevelBuilder(player, _assetProvider, bounds_, environmentStyler);

            Global.Analytics = new YandexAnalytics();
            Global.PlatformSDK = new YGPluginSDKProvider();
            
            levelsLoader.Initialize(_assetProvider);
      }

      private void Start()
      {
            _gameplayFlow = new GameplayFlow(player, cameraContainer, levelsLoader,
                  _levelBuilder, curtain, uiWindowManager, dragController);
            
            bounds.gameObject.SetActive(true);
            curtain.ShowCurtain(BootstrapGame().Forget, true);
      }

      private async UniTaskVoid BootstrapGame()
      {
            var textLocal = await _assetProvider.LoadAsync<TextAsset>("Localization");
            Global.Analytics.GameStarted();
            LocalizationManager.Init(textLocal, Global.PlatformSDK.Language);
            _gameplayFlow.LoadCurrentLevel().Forget();
      }
}