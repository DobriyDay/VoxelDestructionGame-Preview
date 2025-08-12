using System;
using UnityEngine;

public class DragController : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask ignoreLayer;
    [SerializeField] private float dragSpring = 500f;
    [SerializeField] private float dragDamper = 10f;
    [SerializeField] private float cameraDistance = 15f;
    [SerializeField] private LineRenderer lineRenderer;

    private IControllableObject _currentTarget;
    private Rigidbody _draggedRigidbody;
    private Rigidbody _anchorRb;
    private SpringJoint _joint;
    private Transform _anchorTransform;

    public bool CanDrag = true;

    private void Awake()
    {
        lineRenderer.enabled = false;
        var anchorGO = new GameObject("DragAnchor");
        _anchorTransform = anchorGO.transform;
        _anchorRb = anchorGO.AddComponent<Rigidbody>();
        _anchorRb.isKinematic = true;

        _joint = anchorGO.AddComponent<SpringJoint>();
        _joint.autoConfigureConnectedAnchor = false;
        _joint.spring = dragSpring;
        _joint.damper = dragDamper;
        _joint.maxDistance = 0.4f;
        _joint.connectedBody = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(mainCamera.transform.position, Vector3.forward * cameraDistance);
    }

    private void Update()
    {
        if (!CanDrag)
            return;
        
        if (Input.GetMouseButtonDown(0))
            TryStartDrag();

        if (Input.GetMouseButtonUp(0))
            RemoveCurrentTarget();

        if (_draggedRigidbody)
        {
            lineRenderer.SetPosition(0, _draggedRigidbody.transform.position);
            _anchorTransform.position = GetCursorWorldPosition();
            lineRenderer.SetPosition(1, _anchorTransform.position);
        }
    }

    private void TryStartDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 100f, ~ignoreLayer))
            return;

        if (!GameObjectsServiceLocator.TryGet(hit.collider.gameObject, out IControllableObject controllable) || controllable == null
            || (controllable.IsStatic && !controllable.IsStaticUntilTouch))
            return;

        RemoveCurrentTarget();
        _currentTarget = controllable;
        _currentTarget.StartTouch();
        lineRenderer.enabled = true;
        
        _currentTarget.OnChangeControllableState += CurrentTargetOnOnChangeControllableState;
        
        var targetRb = hit.rigidbody;
        if (targetRb == null || targetRb.isKinematic)
            return;

        _draggedRigidbody = targetRb;

        _anchorTransform.position = hit.point;
        _joint.connectedBody = targetRb;
        _joint.connectedAnchor = targetRb.transform.InverseTransformPoint(hit.point);
    }

    public void RemoveCurrentTarget()
    {
        if (_currentTarget != null)
        {
            _currentTarget.OnChangeControllableState -= CurrentTargetOnOnChangeControllableState;
            _currentTarget.EndTouch();
        }
        _currentTarget = null;
        
        lineRenderer.enabled = false;
        _joint.connectedBody = null;
        _draggedRigidbody = null;
        _anchorTransform.position = Vector3.one * 9999f;
    }

    private void CurrentTargetOnOnChangeControllableState(bool isControllable)
    {
        if (!isControllable)
            RemoveCurrentTarget();
    }

    private Vector3 GetCursorWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, mainCamera.transform.position + Vector3.forward * cameraDistance);

        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    }
}
