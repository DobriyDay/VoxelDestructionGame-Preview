using System;
using UnityEngine;

public interface IControllableObject
{
    event Action<bool> OnChangeControllableState;
    
    bool IsControllable { get; }
    bool IsStatic { get; }
    bool IsStaticUntilTouch { get; }
    
    void SetUp(bool isControllable, bool isStatic, bool isStaticUntilTouch);

    void StartTouch();
    void EndTouch();
}