using System;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : CollisionHandler, IInitializableSpawnableObject
{
    [SerializeField] private Vector3 force;
    private HashSet<Rigidbody> _entered = new();
    private Vector3 _transformedForce;
    
    public override void HandleCollisionEnter(Collision collision)
    {
        
    }

    public override void HandleTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            _entered.Add(other.attachedRigidbody);
        }
    }

    public override void HandleTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null)
            _entered.Remove(other.attachedRigidbody);
    }

    private void FixedUpdate()
    {
        if (_entered.Count == 0)
            return;

        foreach (var entered in _entered)
        {
            entered.AddForce(_transformedForce * Time.fixedDeltaTime);
        }
    }

    public void Initialize()
    {
        GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.7f, 1.1f);
        _transformedForce = transform.TransformDirection(force);
    }

    public void Run()
    {
        
    }

    public void Reset()
    {
        _entered.Clear();
    }
}