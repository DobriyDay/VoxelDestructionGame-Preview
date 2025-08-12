using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LaunchGun : MonoBehaviour, IInitializableSpawnableObject
{
    [SerializeField] private float launchForce = 100f;
    [SerializeField] private Transform launchOrigin;
    [SerializeField] private ObjectAudioPlayer launchSound;
    [SerializeField] private ParticleSystem launchParticles;
    
    [Header("Animation")] 
    [SerializeField] private GameObject gunModel;
    [SerializeField] private Vector3 scaleBeforeLaunch;
    [SerializeField] private float scaleBeforeLaunchDuration;
    [SerializeField] private Vector3 launchScale;
    [SerializeField] private float launchDuration;
    [SerializeField] private float returnToNormalDuration;
    [SerializeField] private float pauseAfterShoot = .5f;
    [SerializeField] private float pauseBeforeShoot = .5f;
    private Vector3 _originalScale;
    private Sequence _animationSequence;
    private HashSet<IPunchable> _entered = new HashSet<IPunchable>();
    
    private bool _isLaunching = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (GameObjectsServiceLocator.TryGet(other.gameObject, out IPunchable punchable))
        {
            _entered.Add(punchable);

            if (_isLaunching)
                return;
            
            _isLaunching = true;
            _animationSequence = DOTween.Sequence()
                .SetLink(gameObject);

            _animationSequence.AppendInterval(pauseBeforeShoot);
            _animationSequence.Append(gunModel.transform.DOScale(scaleBeforeLaunch, scaleBeforeLaunchDuration).SetLink(gunModel));
            _animationSequence.Append(gunModel.transform.DOScale(launchScale, launchDuration).SetLink(gunModel).OnComplete(Launch));
            _animationSequence.AppendInterval(pauseAfterShoot);
            _animationSequence.Append(gunModel.transform.DOScale(_originalScale, returnToNormalDuration).SetLink(gunModel).OnComplete(Reset));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (GameObjectsServiceLocator.TryGet(other.gameObject, out IPunchable punchable))
        {
            _entered.Remove(punchable);
        }
    }

    private void Launch()
    {
        launchSound.PlaySound();
        launchParticles.Play();
        foreach (var entered in _entered)
        {
            entered.Punch(launchOrigin.forward * launchForce);
        }
    }

    public void Initialize()
    {
        _originalScale = gunModel.transform.localScale;
    }

    public void Run()
    {
    }

    public void Reset()
    {
        _animationSequence.Kill();
        _entered.Clear();
        _isLaunching = false;
        gunModel.transform.localScale = _originalScale;
    }
}
