using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shootgun : ControllableObject
{
    [SerializeField] private ControllableObject controllableObject;
    [SerializeField] private Transform[] shootPoints;
    [SerializeField] private int damage = 5;
    [SerializeField] private LayerMask ignoreLayers;
    [SerializeField] private ParticleSystem shootParticles;
    [SerializeField] private ParticleSystem ricochetParticles;
    [SerializeField] private ObjectAudioPlayer objectAudioPlayer;
    
    [SerializeField] private float shootDelay;
    private float _shootTimer;
    private bool _shoot;

    public override void StartTouch()
    {
        base.StartTouch();
        _shoot = true;
    }

    public override void EndTouch()
    {
        base.EndTouch();
        _shoot = false;
    }

    private void Update()
    {
        if (!_shoot)
            return;
        
        _shootTimer += Time.deltaTime;
        if (_shootTimer <= shootDelay)
            return;

        _shootTimer = 0;

        Shoot();
    }

    private void Shoot()
    {
        for (int i = 0; i < shootPoints.Length; i++)
        {
            var shootPoint = shootPoints[i];
            
            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, Mathf.Infinity, ~ignoreLayers))
            {
                if (GameObjectsServiceLocator.TryGet(hit.collider.gameObject, out IDamagable damagable))
                {
                    damagable.Damage(damage, hit.point);
                    ricochetParticles.transform.position = hit.point;
                    ricochetParticles.Play();
                }
            }
        }

        
        objectAudioPlayer.PlaySound();
        shootParticles.Play();
    }
}
