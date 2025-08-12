using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public interface IAssetProvider
{
    UniTask<T> LoadAsync<T>(AssetReference address) where T : class;
    UniTask<T> LoadAsync<T>(string address) where T : class;
    void Unload(string address);
    void Unload(AssetReference address);
    void UnloadAll();
}