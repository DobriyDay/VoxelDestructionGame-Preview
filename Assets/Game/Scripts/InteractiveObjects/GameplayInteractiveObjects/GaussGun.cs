using System;
using UnityEngine;

public class GaussGun : MonoBehaviour
{
    [SerializeField] private float range = 100f;
    [SerializeField] private int damage = 50;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float checkInterval = 0.1f;
    [SerializeField] private float shootDelay = 1.8f;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private AudioSource shootSound;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float cameraShakeForce = 1;
    [SerializeField] private float cameraShakeDuration = .2f;
    
    private CameraContainer _cameraContainer;
    private readonly RaycastHit[] _raycastHits = new RaycastHit[16];
    private float _checkTimer;

    private void Start()
    {
        _cameraContainer = CameraContainer.Instance;
    }

    private void Update()
    {
        _checkTimer += Time.deltaTime;
        if (_checkTimer >= checkInterval)
        {
            _checkTimer = 0f;
            CheckAndShoot();
        }
    }

    private void CheckAndShoot()
    {
        Vector3 origin = shootPoint.position;
        Vector3 direction = shootPoint.forward;
        
        int hitCount = Physics.RaycastNonAlloc(origin, direction, _raycastHits, range, hitMask);
        if (hitCount == 0)
            return;

        RaycastHit firstHit = _raycastHits[0];
        if (!GameObjectsServiceLocator.TryGet<PlayerChunks>(firstHit.collider.gameObject, out var playerContainer))
            return;

        if (!playerContainer.IsChunkAlive(firstHit.collider.gameObject))
            return;

        for (int i = 0; i < hitCount; i++)
        {
            print(_raycastHits[i].collider.name);
            if (GameObjectsServiceLocator.TryGet<IDamagable>(_raycastHits[i].collider.gameObject, out var damagable))
            {
                damagable.Damage(damage, origin);
            }
        }

        particles.Play();
        shootSound.pitch = UnityEngine.Random.Range(.8f, 1.2f);
        shootSound.Play();
        _cameraContainer.Shake(cameraShakeDuration, cameraShakeForce);
        _checkTimer -= shootDelay;
    }
}