using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class TNT : MonoBehaviour, IDisposable
{
    [SerializeField] private ChunksContainer chunksContainer;
    [SerializeField] private ExplosiveObject explosionHandler;

    private void OnEnable()
    {
        chunksContainer.OnReseted += explosionHandler.Reset;
        chunksContainer.OnDestroyed += explosionHandler.Reset;
        explosionHandler.OnBeforeExploded += HandleExplosion;
    }

    private void OnDisable()
    {
        Dispose();
    }

    private void OnDestroy()
    {
        Dispose();
    }

    public void Dispose()
    {
        chunksContainer.OnReseted -= explosionHandler.Reset;
        chunksContainer.OnDestroyed -= explosionHandler.Reset;
        explosionHandler.OnBeforeExploded -= HandleExplosion;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (explosionHandler.IsFiring)
            return;

        if (GameObjectsServiceLocator.TryGet(other.gameObject, out EnterableZone zone) && zone is FireZone)
        {
            explosionHandler.TriggerExplosion();
        }
    }

    private void HandleExplosion()
    {
        chunksContainer.DestroyChunks();
    }
}