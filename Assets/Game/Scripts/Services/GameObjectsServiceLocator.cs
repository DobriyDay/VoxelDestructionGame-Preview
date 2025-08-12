using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectsServiceLocator
{
    private static readonly Dictionary<GameObject, Dictionary<Type, object>> Registry 
        = new ();

    private static void CheckForNull(GameObject go)
    {
        if (go == null) 
            throw new ArgumentNullException(nameof(go));
    }
    
    public static void Register<T>(GameObject go, T service) where T : class
    {
        CheckForNull(go);
        if (service == null) 
            throw new ArgumentNullException(nameof(service));
        
        if (!Registry.TryGetValue(go, out var services))
        {
            services = new Dictionary<Type, object>();
            Registry[go] = services;
        }

        services.TryAdd(typeof(T), service);
    }

    public static bool TryGet<T>(GameObject go, out T service) where T : class
    {
        CheckForNull(go);
        
        service = null;
        if (Registry.TryGetValue(go, out var services) &&
            services.TryGetValue(typeof(T), out var obj))
        {
            service = obj as T;
            return true;
        }

        return false;
    }

    public static void Unregister<T>(GameObject go)
    {
        CheckForNull(go);
        
        if (Registry.TryGetValue(go, out var services))
        {
            services.Remove(typeof(T));
            if (services.Count == 0)
                Registry.Remove(go);
        }
    }

    public static void Clear(GameObject go)
    {
        CheckForNull(go);
        Registry.Remove(go);
    }

    public static void ClearAll()
    {
        Registry.Clear();
    }
}