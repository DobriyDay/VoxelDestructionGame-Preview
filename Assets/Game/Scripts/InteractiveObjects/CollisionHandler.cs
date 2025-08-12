using System;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class CollisionHandler : MonoBehaviour
{
       [SerializeField] private bool handleCollisionsByItself = true;
       [SerializeField] private bool handleTriggerByItself = true;

       private void OnCollisionEnter(Collision collision)
       {
              if (handleCollisionsByItself)
                     HandleCollisionEnter(collision);
       }

       private void OnTriggerEnter(Collider other)
       {
              if (handleTriggerByItself)
                     HandleTriggerEnter(other);
       }
       
       private void OnTriggerExit(Collider other)
       {
              if (handleTriggerByItself)
                     HandleTriggerExit(other);
       }

       public abstract void HandleCollisionEnter(Collision collision);
       public abstract void HandleTriggerEnter(Collider other);
       public abstract void HandleTriggerExit(Collider other);
}