using UnityEngine;
using UnityEngine.Serialization;

public class Herobrine : MonoBehaviour
{
    [Header("Shoot settings")]
    [SerializeField] private float capsuleRadius = 0.3f;
    [SerializeField] private float range = 50f;
    [SerializeField] private int damage = 50;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private ObjectAudioPlayer warningSound;
    [SerializeField] private AudioSource shootSound;
    
    [Header("Head Settings")]
    [SerializeField] private Transform head;
    [SerializeField] private float maxYaw = 45f;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Timing")]
    [SerializeField] private float delayBeforeShoot = 3f;
    [SerializeField] private float delayAfterShoot = 3f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem warningParticles;
    [SerializeField] private ParticleSystem shootParticles;
    [SerializeField] private ParticleSystem fireEffect;
    
    private readonly RaycastHit[] _raycastHits = new RaycastHit[16];
    private Quaternion _targetRotation;
    private float _delayBeforeShoot;
    private float _delayAfterShoot;

    private enum State
    {
        RotatingToTarget,
        Shooting,
        Delay
    }

    private State _currentState;

    private void Start()
    {
        SetNewTargetRotation();
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.RotatingToTarget:
                RotateToTargetDirection();
                break;
            case State.Shooting:
                Shoot();
                break;
            case State.Delay:
                DelayAfterShoot();
                break;
        }
    }

    private void DelayAfterShoot()
    {
        _delayAfterShoot += Time.deltaTime;
        if (_delayAfterShoot >= delayAfterShoot)
        {
            SetNewTargetRotation();
        }
    }

    private void Shoot()
    {
        _delayBeforeShoot += Time.deltaTime;
        if (_delayBeforeShoot >= delayBeforeShoot)
        {
            _delayAfterShoot = 0;
            _currentState = State.Delay;
            shootParticles.Play();
            shootSound.pitch = Random.Range(0.8f, 1.1f);
            shootSound.Play();
            
            Vector3 origin = shootPoint.position;
            Vector3 direction = shootPoint.forward;
            Vector3 point1 = origin - shootPoint.up * capsuleRadius;
            Vector3 point2 = origin + shootPoint.up * capsuleRadius;
            
            int hitCount = Physics.CapsuleCastNonAlloc(
                point1,
                point2,
                capsuleRadius,
                direction,
                _raycastHits,
                range,
                hitMask
            );

            for (int i = 0; i < hitCount; i++)
            {
                fireEffect.transform.position = _raycastHits[i].point;
                fireEffect.Play();
                if (GameObjectsServiceLocator.TryGet<IDamagable>(_raycastHits[i].collider.gameObject, out var damagable))
                {
                    damagable.Damage(damage, origin);
                }
            }
        }
    }

    private void RotateToTargetDirection()
    {
        head.localRotation = Quaternion.Slerp(
            head.localRotation,
            _targetRotation,
            Time.deltaTime * rotationSpeed
        );
        if ((head.localRotation.eulerAngles - _targetRotation.eulerAngles).sqrMagnitude <= 1)
        {
            warningSound.PlaySound();
            warningParticles.Play();
            _delayBeforeShoot = 0;
            _currentState = State.Shooting;
        }
    }

    private void SetNewTargetRotation()
    {
        float yaw = Random.Range(-maxYaw, maxYaw);
        _targetRotation = Quaternion.Euler(new Vector3(head.localRotation.eulerAngles.x, yaw, 0f));
        _currentState = State.RotatingToTarget;
    }
}