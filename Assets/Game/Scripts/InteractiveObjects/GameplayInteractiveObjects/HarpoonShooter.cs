using System;
using System.Collections;
using UnityEngine;

public class HarpoonShooter : MonoBehaviour, IInitializableSpawnableObject
{
    [Header("Effects")]
    [SerializeField] private LineRenderer rowLine;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private ObjectAudioPlayer sound;

    [Header("Harpoon Settings")] 
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform harpoonTransform;
    [SerializeField] private float rayDelay = 0.2f;
    [SerializeField] private float harpoonSpeed = 30f;

    [Header("Joint Settings")] 
    [SerializeField] private float dragSpring;
    [SerializeField] private float dragDamper;
    [SerializeField] private float maxDistance;
    
    private Rigidbody _targetRigidbody;
    private Vector3 _hitLocalOffset;
    private bool _isHooked;
    private Coroutine _moveCoroutine;
    private float _rayCastTimer;
    private SpringJoint _joint;

    private void Update()
    {
        if (!_isHooked && _targetRigidbody == null)
        {
            _rayCastTimer += Time.deltaTime;
            if (_rayCastTimer >= rayDelay)
            {
                _rayCastTimer = 0;
                TryShootHarpoon();
            }
        }

        if (_isHooked && _targetRigidbody != null)
        {
            harpoonTransform.position = _targetRigidbody.position +
                                        _targetRigidbody.transform.TransformVector(_hitLocalOffset);
            
            rowLine.SetPosition(1, rowLine.transform.InverseTransformPoint(harpoonTransform.position));
            harpoonTransform.rotation = Quaternion.LookRotation(harpoonTransform.position - shootPoint.position);
        }
    }

    private void TryShootHarpoon()
    {
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.attachedRigidbody == null 
                || !GameObjectsServiceLocator.TryGet<PlayerChunks>(hit.collider.gameObject, out PlayerChunks playerContainer))
                return;

            if (!playerContainer.IsChunkAlive(hit.collider.gameObject))
                return;

            _targetRigidbody = hit.collider.attachedRigidbody;
            if (_targetRigidbody == null)
                return;

            rowLine.enabled = true;
            rowLine.SetPosition(0, shootPoint.localPosition);
            _hitLocalOffset = _targetRigidbody.transform.InverseTransformPoint(hit.point);

            if (_moveCoroutine != null)
                StopCoroutine(_moveCoroutine);

            sound.PlaySound();
            particle.Play();
            _moveCoroutine = StartCoroutine(MoveHarpoonToTarget());
        }
    }

    private IEnumerator MoveHarpoonToTarget()
    {
        Vector3 targetPos = _targetRigidbody.position + 
                           _targetRigidbody.transform.TransformVector(_hitLocalOffset);

        while ((harpoonTransform.position - targetPos).sqrMagnitude > 0.01f)
        {
            harpoonTransform.position = Vector3.MoveTowards(
                harpoonTransform.position, targetPos, harpoonSpeed * Time.deltaTime);
            
            rowLine.SetPosition(1, transform.InverseTransformPoint(harpoonTransform.position));
            
            yield return null;
        }

        _joint.transform.position = shootPoint.position;
        _joint.connectedBody = _targetRigidbody;
        _joint.connectedAnchor = _targetRigidbody.transform.InverseTransformPoint(_targetRigidbody.position);

        _isHooked = true;
    }

    public void Initialize()
    {
        rowLine.enabled = false;
        _targetRigidbody = null;
        
        var anchorGO = new GameObject("DragAnchor");

        _joint = anchorGO.AddComponent<SpringJoint>();
        _joint.GetComponent<Rigidbody>().isKinematic = true;
        _joint.autoConfigureConnectedAnchor = false;
        _joint.spring = dragSpring;
        _joint.damper = dragDamper;
        _joint.maxDistance = maxDistance;
        _joint.connectedBody = null;

        _isHooked = false;
    }

    public void Run()
    {
        
    }

    public void Reset()
    {
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        
        harpoonTransform.position = shootPoint.position;
        harpoonTransform.rotation = shootPoint.rotation;

        _targetRigidbody = null;
        rowLine.enabled = false;
        _isHooked = false;
        Destroy(_joint.gameObject);
        _joint.connectedBody = null;
    }
}
