using System;
using UnityEngine;

public class Turret : MonoBehaviour, IInitializableSpawnableObject
{
    [SerializeField] private DirectionsRotatingObject rotatingObject;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Vector3Int rotatingAxis;
    [SerializeField] private ObjectAudioPlayer shootSounds;
    [SerializeField] private ParticleSystem ricochetParticles;
    [SerializeField] private ParticleSystem shootParticles;
    [SerializeField] private LayerMask ignoreLayers;
    [SerializeField] private int damage = 2;
    [SerializeField] private float shootDelay = 1f;
    [SerializeField] private float forgetDelay = 1f;
    [SerializeField] private float rotateToTargetSpeed = 1f;

    private float _shootTimer;
    private float _forgetTimer;
    private IDamagable _target;
    private Transform _targetTransform;
    private bool _isShooting;
    
    private void Update()
    {
        _shootTimer -= Time.deltaTime;

        if (_shootTimer <= 0f)
        {
            RaycastShoot();
            _shootTimer = shootDelay;
        }

        if (_target == null)
        {
            if (_isShooting)
                TryForgetTarget();
            else
                rotatingObject.transform.localRotation = Quaternion.RotateTowards(rotatingObject.transform.localRotation, Quaternion.identity, rotateToTargetSpeed);
        }
        else if (_target != null)
        {
            RotateToTarget();
        }
    }

    private void RotateToTarget()
    {
        Vector3 desiredDirection = (_targetTransform.position - shootPoint.position).normalized;
        Vector3 currentDirection = shootPoint.forward;

        Quaternion rotationDelta = Quaternion.FromToRotation(currentDirection, desiredDirection);
        rotatingObject.transform.rotation = Quaternion.RotateTowards(rotatingObject.transform.rotation, rotationDelta * rotatingObject.transform.rotation, rotateToTargetSpeed);
    }

    private void TryForgetTarget()
    {
        _forgetTimer += Time.deltaTime;
        if (_forgetTimer >= forgetDelay)
        {
            ForgetTarget();
        }
    }

    private void ForgetTarget()
    {
        _target = null;
        _targetTransform = null;
        _isShooting = false;
        rotatingObject.SetRotatingStatus(true);
        _forgetTimer = forgetDelay;
    }

    private void RaycastShoot()
    {
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, Mathf.Infinity, ~ignoreLayers))
        {
            if (GameObjectsServiceLocator.TryGet(hit.collider.gameObject, out IDamagable damagable))
            {
                rotatingObject.SetRotatingStatus(false);
                _isShooting = true;
                _target = damagable;
                _forgetTimer = 0;
                _targetTransform = hit.transform;
            }
            else
            {
                _target = null;
            }
            if (_isShooting)
                Shoot(hit);
        }
    }

    private void Shoot(RaycastHit hit)
    {
        if (_target != null)
            _target.Damage(damage, hit.point);

        ricochetParticles.transform.position = hit.point;
        ricochetParticles.Play();
                
        shootParticles.Play();

        shootSounds.PlaySound();
    }

    public void Initialize()
    {
        ForgetTarget();
    }

    public void Run()
    {
        
    }

    public void Reset()
    {
        ForgetTarget();
    }
}