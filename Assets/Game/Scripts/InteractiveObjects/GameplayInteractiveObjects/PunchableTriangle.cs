using System;
using UnityEngine;

public class PunchableTriangle : MonoBehaviour, IInitializableSpawnableObject
{
    [SerializeField] private RotatingRigidbodyObject rotatingRigidbody;
    [SerializeField] private PunchObject punchObject;
    [SerializeField] private bool punchableRequired;
    [SerializeField] private bool damageableRequired;
    [SerializeField] private GameObject blockingCollider;
    private Quaternion _defaultRotation;
    
    private void OnTriggerEnter(Collider other)
    {
        if (damageableRequired && GameObjectsServiceLocator.TryGet(other.gameObject, out IDamagable damageable)
            || punchableRequired && GameObjectsServiceLocator.TryGet(other.gameObject, out IPunchable punchable))
        {
            blockingCollider.SetActive(true);
            punchObject.SetPunch(true);
            rotatingRigidbody.SetRotationStatus(true);
        }
    }

    public void Initialize()
    {
        _defaultRotation = rotatingRigidbody.transform.localRotation;
    }

    public void Run()
    {
        rotatingRigidbody.transform.localRotation = _defaultRotation;
    }

    public void Reset()
    {
        blockingCollider.SetActive(false);
        punchObject.SetPunch(false);
        rotatingRigidbody.SetRotationStatus(false);
        rotatingRigidbody.transform.localRotation = _defaultRotation;
    }
}
