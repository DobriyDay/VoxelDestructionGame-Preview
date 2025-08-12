using System;
using UnityEngine;

public class PunchableObject : MonoBehaviour, IPunchable
{
    [SerializeField] private Rigidbody rigidbodyComponent;

    private void OnEnable()
    {
        GameObjectsServiceLocator.Register<IPunchable>(gameObject, this);
    }

    private void OnDestroy()
    {
        GameObjectsServiceLocator.Unregister<IPunchable>(gameObject);
    }

    public void Punch(Vector3 velocity)
    {
        rigidbodyComponent.AddForce(velocity, ForceMode.VelocityChange);
    }
}