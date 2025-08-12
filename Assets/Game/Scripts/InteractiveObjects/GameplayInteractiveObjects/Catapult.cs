using System;
using System.Collections.Generic;
using UnityEngine;

public class Catapult : CollisionHandler, IInitializableSpawnableObject
{
    [SerializeField] private Rigidbody armRigidbody;
    [SerializeField] private Transform arm;
    [SerializeField] private Vector3 shootEuler;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float pauseBeforeShoot = 0.3f;
    [SerializeField] private float pauseAtShoot = 0.3f;
    [SerializeField] private float launchForce = 500f;

    private Quaternion _startRot;
    private Quaternion _shootRot;
    private bool _isActive;
    private float _pauseTimer;
    private State _state;
    private HashSet<IPunchable> _loadedBody = new ();

    private enum State
    {
        Idle,
        RotatingToShoot,
        PauseAtShoot,
        RotatingBack
    }
    
    public override void HandleCollisionEnter(Collision collision) { }

    public override void HandleTriggerEnter(Collider other)
    {
        if (_isActive) return;

        if (GameObjectsServiceLocator.TryGet(other.gameObject, out IPunchable punchable))
        {
            _loadedBody.Add(punchable);
            _isActive = true;
            _pauseTimer = pauseBeforeShoot;
            _state = State.RotatingToShoot;
        }
    }

    public override void HandleTriggerExit(Collider other)
    {
        if (!_isActive) return;

        if (GameObjectsServiceLocator.TryGet(other.gameObject, out IPunchable punchable))
        {
            _loadedBody.Remove(punchable);
        }
    }

    private void FixedUpdate()
    {
        if (!_isActive) return;

        switch (_state)
        {
            case State.RotatingToShoot:
                if (_pauseTimer > 0)
                {
                    _pauseTimer -= Time.fixedDeltaTime;
                    return;
                }

                RotateArm(_shootRot);

                if (IsRotationReached(arm.localRotation, _shootRot))
                {
                    LaunchObject();
                    _pauseTimer = pauseAtShoot;
                    _state = State.PauseAtShoot;
                }
                break;

            case State.PauseAtShoot:
                _pauseTimer -= Time.fixedDeltaTime;
                if (_pauseTimer <= 0)
                {
                    _state = State.RotatingBack;
                }
                break;

            case State.RotatingBack:
                RotateArm(_startRot);

                if (IsRotationReached(arm.localRotation, _startRot))
                {
                    Reset();
                }
                break;
        }
    }

    private void RotateArm(Quaternion target)
    {
        armRigidbody.MoveRotation(
            Quaternion.RotateTowards(
                arm.localRotation,
                target,
                rotateSpeed * Time.fixedDeltaTime
            )
        );
    }

    private bool IsRotationReached(Quaternion a, Quaternion b)
    {
        return Quaternion.Angle(a, b) < 0.5f;
    }

    private void LaunchObject()
    {
        if (_loadedBody.Count != 0)
        {
            foreach (IPunchable punchable in _loadedBody)
            {
                punchable.Punch(arm.forward * launchForce);
            }
        }
    }

    public void Initialize()
    {
        _startRot = arm.localRotation;
        _shootRot = Quaternion.Euler(shootEuler);
    }

    public void Run()
    {
        Reset();
    }

    public void Reset()
    {
        _loadedBody.Clear();
        arm.localRotation = _startRot;
        _isActive = false;
        _state = State.Idle;
    }
}
