using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlyingBarrel : MonoBehaviour, IInitializableSpawnableObject
{
        [SerializeField] private ControllableObject controllableObject;
        [SerializeField] private float detonateTargetDamage = 100;
        [SerializeField] private float flySpeed = 100;
        [SerializeField] private Rigidbody rigidbodyComponent;
        [SerializeField] private ExplosiveObject explodeObject;
        [SerializeField] private GameObject barrelModel;
        private bool _fire = false;
        private bool _exploded = false;
        private Vector3 _startPosition;
        

        private void OnCollisionEnter(Collision other)
        {
                if (_fire || _exploded)
                        return;
                
                if (other.relativeVelocity.sqrMagnitude > detonateTargetDamage)
                {
                        controllableObject.SetUp(false, false, false);
                        explodeObject.TriggerExplosion();
                        _fire = true;
                        StartCoroutine(Fly());
                }
        }

        private IEnumerator Fly()
        {
                float changeInterval = .2f;
                float rotationSpeed = 320f;
                float targetAngle = 0f;
                float elapsed = changeInterval;
                float angleMinimumChange = 145f;

                Transform modelTransform = barrelModel.transform;
                float currentAngle = modelTransform.localRotation.eulerAngles.x;


                while (_fire)
                {
                        yield return new WaitForFixedUpdate();

                        elapsed += Time.fixedDeltaTime;

                        if (elapsed >= changeInterval)
                        {
                                elapsed = 0f;

                                float delta = Random.Range(angleMinimumChange, 360f);
                                if (Random.value > 0.5f)
                                        delta *= -1;

                                targetAngle = Mathf.Repeat(targetAngle + delta, 360f);
                        }

                        currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                        modelTransform.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);

                        rigidbodyComponent.AddForce(-modelTransform.forward * flySpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
        }

        private void OnBeforeExploded()
        {
                _exploded = true;
                rigidbodyComponent.isKinematic = true;
                barrelModel.SetActive(false);
                explodeObject.OnBeforeExploded -= OnBeforeExploded;
                _fire = false;
        }

        public void Initialize()
        {
                _exploded = false;
                barrelModel.SetActive(true);
                _fire = false;
        }

        public void Run()
        {
                explodeObject.OnBeforeExploded += OnBeforeExploded;
                rigidbodyComponent.isKinematic = false;
        }

        private void OnDisable()
        {
                Reset();
        }

        private void OnDestroy()
        {
                Reset();
        }

        public void Reset()
        {
                rigidbodyComponent.isKinematic = true;
                _exploded = false;
                explodeObject.OnBeforeExploded -= OnBeforeExploded;
                explodeObject.Reset();
                explodeObject.StopEffects();
                _fire = false;
        }
}