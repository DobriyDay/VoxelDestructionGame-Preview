using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ExplosiveObject : MonoBehaviour, IExplosionable
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem delayParticle;
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private PulsationEffect pulsationEffect;
    [SerializeField] private ObjectAudioPlayer explodeSound;
    [SerializeField] private ObjectAudioPlayer delaySound;
    
    [Header("Camera shaking")]
    [SerializeField] private float cameraShakeDuration = 0.2f;
    [SerializeField] private float cameraShakeForce = 0.2f;
    
    [Header("Explosion Settings")]
    [SerializeField] private float delayBeforeExplosion = 2f;
    [SerializeField] private int explosionDamage = 100;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private LayerMask damageIgnoreLayers;

    private readonly Collider[] _overlapResults = new Collider[32];
    private Coroutine _explosionCoroutine;
    
    private bool _isExploded;
    public bool IsFiring { get; private set; }
    public event System.Action OnBeforeExploded;

    private void OnDestroy()
    {
        GameObjectsServiceLocator.Unregister<IExplosionable>(gameObject);
    }

    private void OnDisable()
    {
        GameObjectsServiceLocator.Unregister<IExplosionable>(gameObject);
        Reset();
    }

    private void OnEnable()
    {
        GameObjectsServiceLocator.Register<IExplosionable>(gameObject, this);
    }

    public void TriggerExplosion()
    {
        if (IsFiring || _isExploded || !isActiveAndEnabled) return;

        IsFiring = true;
        pulsationEffect?.StartPulse();
        
        _explosionCoroutine = StartCoroutine(FiringCoroutine());
    }

    private IEnumerator FiringCoroutine()
    {
        if (delayBeforeExplosion > 0)
        {
            if (delayParticle != null)
                delayParticle.Play();
            
            delaySound?.PlaySound();
            
            yield return new WaitForSeconds(delayBeforeExplosion);
        }
        
        Explode();
    }

    public void Explode()
    {
        if (_isExploded)
            return;
        
        OnBeforeExploded?.Invoke();

        _isExploded = true;
        CameraContainer.Instance.Shake(cameraShakeDuration, cameraShakeForce);
        pulsationEffect?.StopPulse();
        explosionParticle.transform.rotation = Quaternion.identity;
        explosionParticle.Play();
        explodeSound.PlaySound();

        int hits = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, _overlapResults, ~damageIgnoreLayers);

        for (int i = 0; i < hits; i++)
        {
            var col = _overlapResults[i];
            if (col == null)
                continue;
            
            if (GameObjectsServiceLocator.TryGet(col.gameObject, out IDamagable damagable))
            {
                damagable.Damage(explosionDamage, transform.position);
            }

            if (GameObjectsServiceLocator.TryGet(col.gameObject, out IExplosionable explosionable))
            {
                explosionable.Explode();
            }

            _overlapResults[i] = null;
        }

        IsFiring = false;
    }

    public void StopEffects()
    {
        explosionParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (delayParticle != null)
            delayParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void Reset()
    {
        IsFiring = false;
        _isExploded = false;
        
        if (delayParticle != null)
            delayParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        pulsationEffect?.StopPulse();

        if (_explosionCoroutine != null)
        {
            StopCoroutine(_explosionCoroutine);
            _explosionCoroutine = null;
        }
    }
}