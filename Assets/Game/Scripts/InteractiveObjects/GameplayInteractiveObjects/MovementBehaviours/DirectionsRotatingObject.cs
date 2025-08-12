using UnityEngine;

public class DirectionsRotatingObject : MonoBehaviour
{
    [SerializeField] private Transform rotatingObject;
    [SerializeField] private Vector3[] rotations;
    [SerializeField] private float speed;

    private bool _isRotating = true;
    private Quaternion[] _rotations;
    private int _currentIndex;
    private int _nextIndex;
    
    private void Awake()
    {
        _rotations = new Quaternion[rotations.Length];
        for (int i = 0; i < rotations.Length; i++)
            _rotations[i] = Quaternion.Euler(rotations[i]);
        
        _currentIndex = Random.Range(0, _rotations.Length);
        CalculateNextIndex();
        SetRotatingStatus(true);
    }
    
    
    public void SetRotatingStatus(bool isRotating)
        => _isRotating = isRotating;

    private void CalculateNextIndex()
    {
        _nextIndex = (_currentIndex + 1) % rotations.Length;
    }

    private void Update()
    {
        if (!_isRotating) return;
        
        rotatingObject.localRotation = Quaternion.RotateTowards(rotatingObject.localRotation, _rotations[_nextIndex], speed);

        if ((rotatingObject.localRotation.eulerAngles - _rotations[_nextIndex].eulerAngles).sqrMagnitude < 0.1f)
        {
            _currentIndex = _nextIndex;
            CalculateNextIndex();
        }
    }
}