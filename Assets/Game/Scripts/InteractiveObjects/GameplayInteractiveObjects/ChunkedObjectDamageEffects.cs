using System;
using UnityEngine;

public class ChunkedObjectDamageEffects : MonoBehaviour, IDisposable
{
        [SerializeField] private ChunksContainer chunksContainer;
        [SerializeField] private ParticleSystem damageEffect;
        [SerializeField] private ObjectAudioPlayer objectAudioPlayer;
        
        protected virtual void OnEnable()
        {
                chunksContainer.OnDamagedContainerChunk += ChunksContainerOnDamagedContainerChunk;
                chunksContainer.OnReseted += ContainerOnReset;
        }

        protected virtual void ContainerOnReset()
        {
                if (damageEffect != null)
                {
                        damageEffect.Stop();
                        damageEffect.Clear(); 
                }
        }

        protected virtual void OnDisable()
        {
                Dispose();
        }

        private void OnDestroy()
        {
                Dispose();
        }

        protected virtual void ChunksContainerOnDamagedContainerChunk(Vector3 origin)
        {
                objectAudioPlayer.PlaySound();
                if (damageEffect != null)
                {
                        damageEffect.transform.position = origin;
                        damageEffect.Play();
                }
        }

        public void Dispose()
        {
                chunksContainer.OnReseted -= ContainerOnReset;
                chunksContainer.OnDamagedContainerChunk -= ChunksContainerOnDamagedContainerChunk;
        }
}