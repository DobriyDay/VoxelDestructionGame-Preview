using UnityEngine;
using DG.Tweening;

public class CameraContainer : MonoBehaviour
{
    [System.Serializable]
    public struct LockedAxis
    {
        public bool X;
        public bool Y;
        public bool Z;
    }

    public static CameraContainer Instance;
    
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private Transform target;
    [SerializeField] private Transform shakeTransform;
    [SerializeField] private LockedAxis lockedAxis;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float defaultShakeForce = 0.5f;
    [SerializeField] private float defaultShakeDuration = 0.5f;
    
    private Vector3 _shakeInitialLocalPosition;
    private Tween _shakeTween;

    private void Awake()
    {
        _shakeInitialLocalPosition = shakeTransform.localPosition;
        Instance = this;
    }

    public void Follow(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetOffset(Vector3 offset)
    {
        cameraOffset = offset;
    }

    public void Shake(float duration = -1, float force = -1f)
    {
        if (force <= 0f)
            force = defaultShakeForce;
        if (duration <= 0f)
            duration = defaultShakeDuration;

        _shakeTween?.Kill();

        shakeTransform.localPosition = _shakeInitialLocalPosition;

        _shakeTween = shakeTransform.DOShakePosition(duration, force)
            .SetLink(transform.gameObject)
            .OnComplete(() =>
            {
                shakeTransform.DOLocalMove(_shakeInitialLocalPosition, duration * 0.5f)
                    .SetEase(Ease.OutQuad);
            });
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        MoveToTarget();
    }

    private void MoveToTarget()
    {
        Vector3 movement = target.position;
        
        if (lockedAxis.X)
            movement.x = 0;
        if (lockedAxis.Y)
            movement.y = 0;
        if (lockedAxis.Z)
            movement.z = 0;
        
        transform.position = Vector3.Lerp(transform.position, movement + cameraOffset, followSpeed * Time.deltaTime);
    }
}