using System;
using System.Collections.Generic;
using UnityEngine;

public class Toilet : MonoBehaviour, IInitializableSpawnableObject
{
    [SerializeField] private GameObject toiletModel;
    [SerializeField] private ParticleSystem brakeParticles;
    [SerializeField] private ParticleSystem waterParticles;
    [SerializeField] private ObjectAudioPlayer breakSound;
    [SerializeField] private ObjectAudioPlayer waterPunchSound;
    [SerializeField] private AudioSource waterSound;
    [SerializeField] private float flowUpForce = 15;
    [SerializeField] private float targetSpeed = 3;
    private bool _broken;
    private HashSet<Rigidbody> _rigidbodies = new();
    private Vector3 _center;
    
    
    private void OnCollisionEnter(Collision other)
    {
        if (_broken)
            return;
        
        if (other.relativeVelocity.sqrMagnitude >= targetSpeed * targetSpeed)
        {
            BrakeToilet();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_broken)
            return;

        if (other.attachedRigidbody != null)
        {
            waterPunchSound.PlaySound();
            _rigidbodies.Add(other.attachedRigidbody);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null)
            _rigidbodies.Remove(other.attachedRigidbody);
    }

    private void FixedUpdate()
    {
        if (!_broken)
            return;

        foreach (var rb in _rigidbodies)
        {
            Vector3 force = (_center - rb.position).normalized;
            force.y = 0;
            force += Vector3.up;
            
            Debug.DrawRay(rb.position, force * flowUpForce, Color.red);
            
            rb.AddForce(force * flowUpForce * Time.fixedDeltaTime, ForceMode.Force);
        }
    }

    private void BrakeToilet()
    {
        breakSound.PlaySound();
        waterSound.Play();
        _center = transform.position;
        
        _broken = true;
        toiletModel.SetActive(false);
        brakeParticles.Play();
        waterParticles.Play();
    }

    public void Initialize()
    {
        _broken = false;
    }

    public void Run()
    {
    }

    public void Reset()
    {
        waterSound.Stop();
        _rigidbodies.Clear();
        brakeParticles.Stop();
        waterParticles.Stop();
        toiletModel.SetActive(true);
    }
}
