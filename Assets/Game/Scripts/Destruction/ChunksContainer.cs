using System;
using System.Collections;
using UnityEngine;

public class ChunksContainer : MonoBehaviour, IDisposable, IInitializableSpawnableObject, IControllableObject
{
        [SerializeField] protected Chunk[] chunks;
        public event Action<bool> OnChangeControllableState;
        public bool IsControllable => isControllable;
        public bool IsStatic => isStatic;
        public bool IsStaticUntilTouch => isStaticUntilTouch;
        private bool isDestroyed = false;


        [SerializeField] private bool isControllable;
        [SerializeField] private bool isStatic;
        [SerializeField] private bool isStaticUntilTouch;
        [SerializeField] [Min(0)] private int chunksLeftToDestroy = 2;

        public event Action OnDestroyed;
        public event Action OnReseted;
        public event Action<Vector3> OnDamagedContainerChunk;
        
        public void Initialize()
        {
                SubscribeToChunks();
        }
        
        public void SetUp(bool isControllable, bool isStatic, bool isStaticUntilTouch)
        {
                this.isControllable = isControllable;
                this.isStatic = isStatic;
                this.isStaticUntilTouch = isStaticUntilTouch;
                for (int i = 0; i < chunks.Length; i++)
                {
                        chunks[i].SetUp(isControllable, isStatic, isStaticUntilTouch);
                }
                OnChangeControllableState?.Invoke(isControllable);
        }

        public void StartTouch()
        {
                if (IsStaticUntilTouch && isStatic)
                {
                        isStatic = false;
                        for (int i = 0; i < chunks.Length; i++)
                        {
                                chunks[i].StartTouch();
                        }
                }
        }

        public void EndTouch()
        {
                
        }

        private void OnDamagedSomeChunk(Vector3 origin)
        {
                OnDamagedContainerChunk?.Invoke(origin);  
        }

        public void Run()
        {
                isDestroyed = false;
                for (int i = 0; i < chunks.Length; i++)
                {
                        chunks[i].RunChunkPhysics();
                } 
        }

        public void DestroyChunks()
        {
                if (!isDestroyed)
                        return;
                
                isDestroyed = true;
                for (int i = 0; i < chunks.Length; i++)
                        chunks[i].Kill();
        }
        
        public void Reset()
        {
                UnsubscribeFromChunks();
                OnReseted?.Invoke();
        }

        private void SubscribeToChunks()
        {
                for (int i = 0; i < chunks.Length; i++)
                {
                        chunks[i].Initialize();
                        chunks[i].OnChunkIsDestroyed += OnChunkDestroyed;
                        chunks[i].OnChunkDamaged += OnDamagedSomeChunk;
                        chunks[i].OnChunkPunched += Punch;
                } 
        }

        private void UnsubscribeFromChunks()
        {
                for (int i = 0; i < chunks.Length; i++)
                {
                        chunks[i].OnChunkIsDestroyed -= OnChunkDestroyed;
                        chunks[i].OnChunkDamaged -= OnDamagedSomeChunk;
                        chunks[i].OnChunkPunched -= Punch;
                        chunks[i].StopChunkPhysics();
                        chunks[i].Restore();
                } 
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                        return;
#endif
                Dispose();  
        }

        public bool IsChunkAlive(GameObject chunkObject)
        {
                foreach (var chunk in chunks)
                {
                        if (chunk.gameObject == chunkObject)
                        {
                                return chunk.IsAlive;
                        }
                }

                return false;
        }

        private void OnChunkDestroyed()
        {
                if (isDestroyed) 
                        return;
                
                isDestroyed = false;
                int destroyCount = 0;
                for (int i = 0; i < chunks.Length; i++)
                {
                        if (!chunks[i].IsAlive)
                        {
                                destroyCount++;
                                if (chunks.Length - destroyCount <= chunksLeftToDestroy)
                                {
                                        isDestroyed = true;
                                        break;
                                }
                        }
                }
                
                if (isDestroyed)
                {
                        for (int i = 0; i < chunks.Length; i++)
                        {
                                chunks[i].Kill();
                                chunks[i].StopChunkPhysics();
                        }
                        
                        OnDestroyed?.Invoke();
                }
        }

        public virtual void Dispose()
        {
                UnsubscribeFromChunks();
                GameObjectsServiceLocator.Unregister<IPunchable>(gameObject);
                OnReseted = null;
                OnDestroyed = null;
                OnDamagedContainerChunk = null;
                OnChangeControllableState = null;
        }

        public void Punch(Vector3 velocity)
        {
                for (int i = 0; i < chunks.Length; i++)
                {
                        chunks[i].HandlePunch(velocity);
                }   
        }
}