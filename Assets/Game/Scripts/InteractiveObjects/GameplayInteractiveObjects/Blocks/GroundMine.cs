using System;
using System.Collections;
using UnityEngine;

public class GroundMine : MonoBehaviour, IInitializableSpawnableObject
{
        [SerializeField] private GameObject mineModel;
        [SerializeField] private ExplosiveObject explosiveObject;
        
        private void OnTriggerEnter(Collider other)
        {
                if (GameObjectsServiceLocator.TryGet(other.gameObject, out IDamagable damagableObject))
                {
                        mineModel.SetActive(false);
                        explosiveObject.TriggerExplosion();
                }
        }

        public void Initialize()
        {
                mineModel.SetActive(true);
        }

        public void Run()
        {
                
        }

        public void Reset()
        {
                explosiveObject.Reset();
                mineModel.SetActive(true);
        }
}