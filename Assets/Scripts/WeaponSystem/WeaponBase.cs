using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected string weaponName = "Weapon";
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 2f; // раз/сек

    protected float cd;

    public string WeaponName => weaponName;
    public bool CanFire => cd <= 0f;
    public float FireRate => fireRate;

    public virtual void Tick(float dt)
    {
        cd -= dt;
    }

    public void Fire(AimContext aim)
    {
        if (!CanFire) return;
        cd = 1f / Mathf.Max(0.0001f, fireRate);
        OnFire(aim);
    }

    protected abstract void OnFire(AimContext aim);

    // Для апгрейдов
    public void AddDamage(float add) => damage += add;
    public void MulDamage(float mul) => damage *= mul;
    public void AddFireRate(float add) => fireRate += add;
}
