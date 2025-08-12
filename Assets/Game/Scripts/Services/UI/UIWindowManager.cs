using System;
using System.Collections.Generic;
using UnityEngine;

public class UIWindowManager : MonoBehaviour
{
    [SerializeField] private List<BaseWindow> windows;
    private readonly Dictionary<Type, BaseWindow> _windowMap = new();
    
    private void Awake()
    {
        foreach (var window in windows)
        {
            _windowMap[window.GetType()] = window;
            window.Hide();
        }
    }

    public T Get<T>() where T : BaseWindow
    {
        if (_windowMap.TryGetValue(typeof(T), out var window))
            return window as T;

        Debug.LogError($"[UIWindowManager] Window of type {typeof(T)} not found");
        return null;
    }

    public void Show<T>(bool hideOther) where T : BaseWindow
    {
        if (hideOther)
        {
            HideAll();
        }
        
        Get<T>()?.Show();
    }

    public void HideAll()
    {
        for (int i = 0; i < windows.Count; i++)
        {
            if (windows[i].WindowIsActive)
            {
                windows[i].Hide();
            }
        }
    }

    public void Hide<T>() where T : BaseWindow 
        => Get<T>()?.Hide();
}