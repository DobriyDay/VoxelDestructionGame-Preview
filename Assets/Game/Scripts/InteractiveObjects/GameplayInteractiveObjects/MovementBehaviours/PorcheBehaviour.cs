using System;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class PorcheBehaviour : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float durationToA = 1f;
    [SerializeField] private float durationToB = 2f;
    [SerializeField] private float pauseAtPoints = 1f;
    [SerializeField] private Vector3 sizeA = Vector3.one;
    [SerializeField] private Vector3 sizeB = Vector3.one * 1.5f;
    [SerializeField] private PunchObject punchObject;
    [SerializeField] private DamageObject damageObject;

    [Header("Pressure")]
    [SerializeField] private float checkDistance = 0.5f;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask damageableLayer;
    [SerializeField] private LayerMask ignoreLayerToCheckObstaclesInFront;

    private readonly Collider[] _overlapBuffer = new Collider[12];

    private void Start()
    {
        transform.localPosition = pointA.localPosition;
        transform.localScale = sizeA;
        MoveTo(pointB, durationToB, sizeB);
    }

    private void MoveTo(Transform target, float duration, Vector3 targetScale)
    {
        if (punchObject != null)
            punchObject.SetPunch(target == pointB);
        if (damageObject != null)
            damageObject.SetDamage(target == pointB);

        Sequence seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(transform.DOLocalMove(target.localPosition, duration).SetEase(Ease.Linear));
        seq.Join(transform.DOScale(targetScale, duration).SetEase(Ease.Linear));
        seq.AppendCallback(() =>
        {
            if (target == pointB)
                TryDamageInFront();
        });
        seq.AppendInterval(pauseAtPoints);

        seq.OnComplete(() =>
        {
            if (target == pointA)
                MoveTo(pointB, durationToB, sizeB);
            else
                MoveTo(pointA, durationToA, sizeA);
        });
    }

    private void OnDrawGizmosSelected()
    {
        if (!pointB) return;

        Gizmos.color = Color.yellow;
        Vector3 origin = pointB.position;
        Vector3 direction = pointB.forward;

        Gizmos.DrawWireSphere(origin + direction * checkDistance, checkRadius);
        Gizmos.DrawRay(origin, direction * checkDistance);
    }
    
    private void TryDamageInFront()
    {
        Vector3 origin = pointB.position;
        Vector3 direction = pointB.forward;
        Vector3 checkPoint = origin + direction * checkDistance;
        
        if (!Physics.Raycast(origin, direction, out RaycastHit obstacleHit, checkDistance, ~ignoreLayerToCheckObstaclesInFront))
        {
            return;
        }

        int count = Physics.OverlapSphereNonAlloc(
            checkPoint,
            checkRadius,
            _overlapBuffer,
            damageableLayer
        );

        for (int i = 0; i < count; i++)
        {
            GameObject go = _overlapBuffer[i].gameObject;
            if (GameObjectsServiceLocator.TryGet(go, out IDamagable damagable))
            {
                damagable.Kill();
            }

            _overlapBuffer[i] = null;
        }
    }
}
