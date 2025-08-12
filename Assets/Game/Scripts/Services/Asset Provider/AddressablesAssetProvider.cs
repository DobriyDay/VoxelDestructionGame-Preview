using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class AddressablesAssetProvider : IAssetProvider
{
    private readonly Dictionary<string, object> _cache = new();
    private readonly Dictionary<string, UniTask<object>> _pendingLoads = new();
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

    public UniTask<T> LoadAsync<T>(AssetReference address) where T : class
    {
        return LoadAsync<T>(address.AssetGUID);
    }

    public void Unload(AssetReference address)
    {
        Unload(address.AssetGUID);
    }
    
    public async UniTask<T> LoadAsync<T>(string address) where T : class
    {
        if (_cache.TryGetValue(address, out var cached))
            return cached as T;

        if (_pendingLoads.TryGetValue(address, out var existingTask))
            return await existingTask as T;

        var handle = Addressables.LoadAssetAsync<T>(address);
        var task = HandleLoad(handle, address);

        _pendingLoads[address] = task;
        return await task as T;
    }
    
    private async UniTask<object> HandleLoad<T>(AsyncOperationHandle<T> handle, string address) where T : class
    {
        await handle.ToUniTask();

        _pendingLoads.Remove(address);

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load asset: {address}");
            return null;
        }

        _cache[address] = handle.Result;
        _handles[address] = handle;
        return handle.Result;
    }

    public void Unload(string address)
    {
        if (_handles.TryGetValue(address, out var handle))
        {
            Addressables.Release(handle);
            _handles.Remove(address);
        }

        _cache.Remove(address);
    }

    public void UnloadAll()
    {
        foreach (var handle in _handles.Values)
            Addressables.Release(handle);

        _handles.Clear();
        _cache.Clear();
    }
}