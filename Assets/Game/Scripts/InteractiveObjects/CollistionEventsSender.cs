using UnityEngine;

public class CollistionEventsSender : MonoBehaviour
{
    [SerializeField] private CollisionHandler[] handlers;
       
    private void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < handlers.Length; i++)
        {
            handlers[i].HandleCollisionEnter(collision);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < handlers.Length; i++)
        {
            handlers[i].HandleTriggerEnter(other);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < handlers.Length; i++)
        {
            handlers[i].HandleTriggerExit(other);
        }
    }
}