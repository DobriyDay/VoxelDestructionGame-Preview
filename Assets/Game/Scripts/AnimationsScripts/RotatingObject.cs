using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [SerializeField] private Vector3 rotation;

    private void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }
}
