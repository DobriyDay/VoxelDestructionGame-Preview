using System;
using UnityEngine;

public class DamageObject : CollisionHandler
{
    private enum DamageType
    {
        Permanent = 0,
        Once = 1,
        Kill = 2
    }

    [SerializeField] private ParticleSystem damageEffect;
    [SerializeField] private bool canDamage = true;
    [SerializeField] private int damage;
    [SerializeField] private float permanentDamageDelay = 1;
    [SerializeField] private float minimumSpeedToDamage = 1;
    [SerializeField] private DamageType damageType = DamageType.Once;
    [SerializeField] private ObjectAudioPlayer objectAudioPlayer;
    [SerializeField] private ObjectAudioPlayer missHitAudioPlayer;
    private float _lastDamageTime;

    public void SetDamage(bool canDamage)
    {
        this.canDamage = canDamage;
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (!canDamage || damageType != DamageType.Permanent || Time.time - _lastDamageTime < permanentDamageDelay)
            return;
     
        if (GameObjectsServiceLocator.TryGet(other.gameObject, out IDamagable damagable))
        {
            Damage(damagable, transform.position);
            _lastDamageTime = Time.time;
        }
    }

    private void Damage(IDamagable damagable, Vector3 damageOrigin)
    {
        objectAudioPlayer.PlaySound();
        if (damageEffect != null)
            damageEffect.Play();
        
        if (damageType == DamageType.Once || damageType == DamageType.Permanent)
            damagable.Damage(damage, damageOrigin);
        else if (damageType == DamageType.Kill)
            damagable.Kill();
    }

    public override void HandleCollisionEnter(Collision collision)
    {
        if (!canDamage || collision.relativeVelocity.sqrMagnitude < minimumSpeedToDamage * minimumSpeedToDamage)
            return;
        
        if (GameObjectsServiceLocator.TryGet(collision.gameObject, out IDamagable damagable))
        {
            Damage(damagable, collision.GetContact(0).point);
        }
        else
        {
            missHitAudioPlayer?.PlaySound();
        }
    }

    public override void HandleTriggerEnter(Collider other)
    {
        
    }

    public override void HandleTriggerExit(Collider other)
    {
        
    }
}
