using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class ChunkPart : MonoBehaviour
{
    public bool IsDetached { get; private set; }
    [SerializeField] private Rigidbody rigidbodyComponent;
    [SerializeField] private MeshCollider meshCollider;
    private Vector3 _initialLocalPosition;
    private Vector3 _initialLocalScale;
    private Quaternion _initialLocalRotation;
    private Transform _originalParent;

    private void OnValidate()
    {
        if (rigidbodyComponent == null)
        {
            gameObject.layer = LayerMask.NameToLayer("ChunkPart");
            rigidbodyComponent = GetComponent<Rigidbody>();
            rigidbodyComponent.isKinematic = true;
            rigidbodyComponent.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();

            meshCollider.convex = true;
            meshCollider.enabled = false;
        }
    }

    public void Initialize()
    {
        IsDetached = false;
        _initialLocalPosition = transform.localPosition;
        _initialLocalRotation = transform.localRotation;
        _initialLocalScale = transform.localScale;
        _originalParent = transform.parent;
        rigidbodyComponent.isKinematic = true;
    }

    public void Detach(Vector3 explosionOrigin, float force)
    {
        meshCollider.enabled = true;
        transform.parent = null;
        rigidbodyComponent.isKinematic = false;
        rigidbodyComponent.linearVelocity = Vector3.zero;
        rigidbodyComponent.angularVelocity = Vector3.zero;
        rigidbodyComponent.AddExplosionForce(force * 10, explosionOrigin, 10f);
        IsDetached = true;
    }

    public void ResetState()
    {
        meshCollider.enabled = false;

        if (!rigidbodyComponent.isKinematic)
        {
            rigidbodyComponent.linearVelocity = Vector3.zero;
            rigidbodyComponent.angularVelocity = Vector3.zero;
            rigidbodyComponent.isKinematic = true;
        }

        transform.parent = _originalParent;
        transform.localPosition = _initialLocalPosition;
        transform.localRotation = _initialLocalRotation;
        transform.localScale = _initialLocalScale;
        IsDetached = false;
    }
}