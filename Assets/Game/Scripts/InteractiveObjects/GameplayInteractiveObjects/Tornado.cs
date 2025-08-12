using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Tornado : MonoBehaviour, IInitializableSpawnableObject
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float liftSpeed = 2f;
    [SerializeField] private float maxPullForce = 10f;
    private float _radius = 5f;
    [SerializeField] private float orbitRadius = 3f;
    
    
    private Dictionary<Rigidbody, RigidbodyConstraints> _savedConstraints = new Dictionary<Rigidbody, RigidbodyConstraints>();
    private readonly HashSet<Rigidbody> _bodies = new();

    private void Awake()
    {
        var col = GetComponent<CapsuleCollider>();
        col.isTrigger = true;
        _radius = col.radius;
        _radius *= _radius;
    }

    private void OnDisable()
    {
        _bodies.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null && !_bodies.Contains(other.attachedRigidbody))
        {
            _savedConstraints[other.attachedRigidbody] = other.attachedRigidbody.constraints;
            other.attachedRigidbody.constraints = RigidbodyConstraints.None;
            
            _bodies.Add(other.attachedRigidbody);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null && _bodies.Contains(other.attachedRigidbody))
        {
            _bodies.Remove(other.attachedRigidbody);
            other.attachedRigidbody.constraints = _savedConstraints[other.attachedRigidbody];
            _savedConstraints.Remove(other.attachedRigidbody);
        }
    }

    private void RemoveBodies()
    {
        foreach (var body in _bodies)
        {
            body.constraints = _savedConstraints[body];
        }
        _bodies.Clear();
        _savedConstraints.Clear();
    }
    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;
        
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        float colRadius = GetComponent<CapsuleCollider>() ? GetComponent<CapsuleCollider>().radius : 5f;
        Gizmos.DrawWireSphere(transform.position, colRadius);
    }

    
    private void FixedUpdate()
    {
        foreach (var body in _bodies)
        {
            if (body == null) continue;

            Vector3 center = transform.position;
            Vector3 toBody = body.position - center;
            toBody.y = 0;

            float distance = toBody.sqrMagnitude;
            if (distance > _radius) continue;

            Vector3 dir = toBody.normalized;


            float radialOffset = distance - (orbitRadius * orbitRadius);
            Vector3 radialForce = (radialOffset > 0)
                ? -dir * (radialOffset * maxPullForce)
                : dir * (-radialOffset * maxPullForce);
            
            body.AddForce(radialForce, ForceMode.Acceleration);

            Vector3 tangential = Vector3.Cross(Vector3.up, dir);
            body.AddForce(tangential * rotationSpeed, ForceMode.Acceleration);
            
            if (body.position.y < transform.position.y)
                body.AddForce(Vector3.up * liftSpeed, ForceMode.Acceleration);
        }
    }

    public void Initialize()
    {
    }

    public void Run()
    {
        _bodies.Clear();
        _savedConstraints.Clear();
    }

    public void Reset()
    {
        RemoveBodies();
    }
}
