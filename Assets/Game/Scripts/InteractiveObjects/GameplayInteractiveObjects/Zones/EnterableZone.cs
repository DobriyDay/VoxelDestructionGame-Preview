using System;
using UnityEngine;

public abstract class EnterableZone : MonoBehaviour
{
        private void OnEnable()
        {
                GameObjectsServiceLocator.Register<EnterableZone>(gameObject, this);
        }

        private void OnDisable()
        {
                GameObjectsServiceLocator.Unregister<EnterableZone>(gameObject);
        }
}