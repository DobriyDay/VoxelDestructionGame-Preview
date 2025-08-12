using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PunchObject : CollisionHandler
{
        [SerializeField] private ObjectAudioPlayer sound;
        [SerializeField] private float force;
        [SerializeField] private bool punch;

        public void SetPunch(bool punch)
        {
                this.punch = punch;
        }

        public override void HandleCollisionEnter(Collision collision)
        {
                if (!punch || force <= 0)
                        return;
                
                if (GameObjectsServiceLocator.TryGet(collision.gameObject, out IPunchable punchable))
                {
                        var contact = -collision.GetContact(0).normal;
                        punchable.Punch(force * contact);
                        sound.PlaySound();
                }  
        }

        public override void HandleTriggerEnter(Collider other)
        {
                
        }

        public override void HandleTriggerExit(Collider other)
        {
                
        }
}