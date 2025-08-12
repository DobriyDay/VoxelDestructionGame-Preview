using System;
using UnityEngine;

public  class ControllableObject : MonoBehaviour, IControllableObject, IDisposable
{
    [SerializeField] private bool isControllable = true;
    [SerializeField] private bool isStatic = false;
    [SerializeField] private bool isStaticUntilTouch = false;
    [SerializeField] protected Rigidbody rigidbodyComponent;

    public event Action<bool> OnChangeControllableState;
    public bool IsControllable => isControllable;
    public bool IsStatic => isStatic;
    public bool IsStaticUntilTouch => isStaticUntilTouch;
    
    private bool _isStaticDefault;

    public void SetUp(bool isControllable, bool isStatic, bool isStaticUntilTouch)
    {
        _isStaticDefault = isStatic;
        this.isStaticUntilTouch = isStaticUntilTouch;
        this.isControllable = isControllable;
        this.isStatic = isStatic;

        if (isStatic && !isStaticUntilTouch)
        {
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
        
        rigidbodyComponent.isKinematic = isStatic;
        OnChangeControllableState?.Invoke(isControllable);
    }

    public virtual void StartTouch()
    {
        if (isStaticUntilTouch && isStatic)
        {
            isStatic = false;
            rigidbodyComponent.isKinematic = isStatic;
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    public virtual void EndTouch()
    {
        
    }

    private void OnDestroy()
    {
        Dispose();
    }

    private void OnEnable()
    {
        GameObjectsServiceLocator.Register<IControllableObject>(gameObject, this);
    }

    private void OnDisable()
    {
        Dispose();
    }

    public void Dispose()
    {
        OnChangeControllableState = null;
        GameObjectsServiceLocator.Unregister<IControllableObject>(gameObject);
    }
}