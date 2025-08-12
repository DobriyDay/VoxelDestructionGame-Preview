using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class MovingBetweenObstacles : MonoBehaviour
{
    [SerializeField] private Transform model;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private float rotationSpeed = 50;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private float turnCooldown = 0.2f;
    [SerializeField] private LayerMask ignoreLayers;
    
    [SerializeField] private Vector3[] directions = new []
    {
        Vector3.forward,
        Vector3.right,
        Vector3.back,
        Vector3.left
    };

    private Rigidbody _rb;
    private int _currentDirectionIndex;
    private float _lastTurnTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 direction = directions[_currentDirectionIndex].normalized;
        Vector3 origin = raycastOrigin.position;
        
        if (Physics.Raycast(origin, direction, rayDistance, ~ignoreLayers))
        {
            print(123);
            if (Time.time - _lastTurnTime > turnCooldown)
            {
                _currentDirectionIndex = (_currentDirectionIndex + 1) % directions.Length;
                _lastTurnTime = Time.time;
            }
            return;
        }
        

        _rb.MovePosition(_rb.position + direction * speed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        RotateBody(directions[_currentDirectionIndex].normalized);

    }

    private void RotateBody(Vector3 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        model.rotation = Quaternion.Slerp(model.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (directions == null) return;

        Gizmos.color = Color.red;
        Vector3 origin = transform.position;

        foreach (var dir in directions)
        {
            Gizmos.DrawRay(origin, dir.normalized * rayDistance);
        }
    }
#endif
}