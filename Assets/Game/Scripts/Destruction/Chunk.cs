using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Chunk : MonoBehaviour, IDamagable, IControllableObject, IPunchable
{
    public event Action OnChunkIsDestroyed;
    public event Action<Vector3> OnChunkDamaged;
    public event Action<Vector3> OnChunkPunched;

    private int _maxHealth = 500;
    [SerializeField] private float speedToDamageFactor = .01f;
    [SerializeField] [Range(5, 100)] private float percentDetachedToDestroy = 40f;
    [SerializeField] private Collider[] neighbors;
    [SerializeField] private ChunkPart[] chunkParts;
    [SerializeField] private GameObject preview;
    [SerializeField] private GameObject realChunks;
    private bool _isControllable = true;
    private bool _isStatic = false;
    private bool _isStaticUntilTouch = false;

    public event Action<bool> OnChangeControllableState;

    public bool IsControllable 
        => _isControllable;

    public bool IsStatic 
        => _isStatic;

    public bool IsStaticUntilTouch
        => _isStaticUntilTouch;


    [field :SerializeField] public int CurrentHealth { get; private set; }
    private bool _isAlive = true;

    private HashSet<Collider> _neighborsToIgnore = new();
    private Collider _collider;
    private Rigidbody _rigidbody;
    private Vector3 _defaultPosition;
    private Vector3 _defaultScale;
    private Quaternion _defaultRotation;

    public bool IsAlive => _isAlive;

    private bool _isInitialized;
    
    public void Initialize()
    {
        if (_isInitialized)
            return;
        
        for (int i = 0; i < chunkParts.Length; i++)
            chunkParts[i].Initialize();
        
        
        _defaultPosition = transform.localPosition;
        _defaultRotation = transform.localRotation;
        _defaultScale = transform.localScale;
        
        _maxHealth = chunkParts.Length;
        CurrentHealth = _maxHealth;
        
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _neighborsToIgnore = new HashSet<Collider>(neighbors);
        neighbors = null;

        Restore();
        _rigidbody.isKinematic = true;
        _isInitialized = true;
    }

    private void SwitchPreviewVisibility(bool visible)
    {
        if (preview != null)
        {
            preview.SetActive(visible);
            realChunks.SetActive(!visible);
        }
    }
    
    public void SetUp(bool isControllable, bool isStatic, bool isStaticUntilTouch)
    {
        this._isStaticUntilTouch = isStaticUntilTouch;
        this._isControllable = isControllable;
        this._isStatic = isStatic;
        
        if (isStatic && !isStaticUntilTouch)
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        OnChangeControllableState?.Invoke(isControllable);
    }

    public void StartTouch()
    {
        if (IsStaticUntilTouch && _isStatic)
        {
            _isStatic = false;
            gameObject.layer = LayerMask.NameToLayer("Default");
            RunChunkPhysics();
        }
    }

    public void EndTouch()
    {
        
    }

    private void RemoveFromLocator()
    {
        GameObjectsServiceLocator.Unregister<IControllableObject>(gameObject);
        GameObjectsServiceLocator.Unregister<IDamagable>(gameObject);
        GameObjectsServiceLocator.Unregister<IPunchable>(gameObject);
    }
    
    private void AddToLocator()
    {
        GameObjectsServiceLocator.Register<IPunchable>(gameObject, this);
        GameObjectsServiceLocator.Register<IControllableObject>(gameObject, this);
        GameObjectsServiceLocator.Register<IDamagable>(gameObject, this);
    }

    private void OnDestroy()
    {
        RemoveFromLocator();
        OnChunkPunched = null;
        OnChunkIsDestroyed = null;
        OnChunkDamaged = null;
        OnChangeControllableState = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_isAlive || _neighborsToIgnore.Contains(collision.collider) || collision.gameObject.CompareTag("DamageFriendly"))
            return;

        float speed = collision.relativeVelocity.sqrMagnitude * .01f;

        int damage = Mathf.RoundToInt(speed * speedToDamageFactor);

        if (damage > 0)
            Damage(damage, collision.contacts[0].point);
    }

    public void Damage(int damage, Vector3 damageOrigin)
    {
        if (!_isAlive)
            return;
        
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, _maxHealth);
        DetachClosestParts(damageOrigin, damage);
        OnChunkDamaged?.Invoke(damageOrigin);
        
        if (CurrentHealth <= 0 || CurrentHealth <= _maxHealth * percentDetachedToDestroy / 100)
            Kill();
    }
    
    private void DetachClosestParts(Vector3 origin, int damage)
    {
        if (chunkParts == null || chunkParts.Length == 0)
            return;

        SwitchPreviewVisibility(false);

        var sorted = chunkParts
            .Where(p => p != null && !p.IsDetached)
            .OrderBy(p => Vector3.SqrMagnitude(p.transform.position - origin))
            .ToList();

        int toDetach = Mathf.Min(damage, sorted.Count);

        for (int i = 0; i < toDetach; i++)
        {
            sorted[i].Detach(origin, damage);
        }
    }
    
    public void Kill()
    {
        if (!_isAlive)
            return;
        
        SwitchPreviewVisibility(false);
        
        foreach (var part in chunkParts)
        {
            if (part != null && !part.IsDetached)
            {
                part.Detach(transform.position, 1);
            }
        }

        _isAlive = false;
        _collider.enabled = false;

        RemoveFromLocator();
        
        OnChunkIsDestroyed?.Invoke();
    }

    public void Restore()
    {
        foreach (var part in chunkParts)
            part.ResetState();

        CurrentHealth = _maxHealth;
        transform.localPosition = _defaultPosition;
        transform.localRotation = _defaultRotation;
        transform.localScale = _defaultScale;
        _isAlive = true;
        SwitchPreviewVisibility(true);
    }

    public void RunChunkPhysics()
    {
        _collider.enabled = true;
        AddToLocator();
        _rigidbody.isKinematic = _isStatic;
    }

    public void StopChunkPhysics()
    {
        _collider.enabled = false;
        _rigidbody.isKinematic = true;
    }
        
    public void Punch(Vector3 velocity)
    {
        OnChunkPunched?.Invoke(velocity);
    }

    public void HandlePunch(Vector3 velocity)
    {
        OnChangeControllableState?.Invoke(false);

        _rigidbody.AddForce(velocity, ForceMode.Impulse);
    }

#if UNITY_EDITOR
    [ContextMenu("Collect Parts")]
    private void CollectParts()
    {
        var parts = new List<ChunkPart>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (!child.TryGetComponent(out ChunkPart part))
                part = child.gameObject.AddComponent<ChunkPart>();

            parts.Add(part);
        }

        chunkParts = parts.ToArray();
        EditorUtility.SetDirty(this);
    }
#endif
}
