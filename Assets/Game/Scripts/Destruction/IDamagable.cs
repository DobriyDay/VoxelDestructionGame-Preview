using UnityEngine;

public interface IDamagable
{
    public int CurrentHealth { get; }
    void Damage(int damage, Vector3 damageOrigin);
    void Restore();
    void Kill();
}