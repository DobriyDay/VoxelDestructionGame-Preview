using System;
using UnityEngine;

public class RotatingRigidbodyObject : MonoBehaviour
{
    [SerializeField] private bool rotate = true;
    [SerializeField] private Rigidbody rigidbodyComponent;
    [SerializeField] private Vector3 rotation;
    private Quaternion _rotationQuaternion;
    
    private void Awake()
    {
        _rotationQuaternion = Quaternion.Euler(rotation);
    }
    
    public void SetRotationStatus(bool rotate)
        => this.rotate = rotate;
    
    private void FixedUpdate()
    {
        if (!rotate) 
            return;
        
        rigidbodyComponent.MoveRotation(transform.rotation * _rotationQuaternion);
    }
}
