using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 2f; // раз/сек
    [SerializeField] protected float skillCooldown = 8f;
    protected string weaponName = "Weapon";

    protected float cd;
    protected float skillCd;

    public string WeaponName => weaponName;
    public bool CanFire => cd <= 0f;
    public bool CanUseSkill => skillCd <= 0f;
    public float FireRate => fireRate;

    public virtual void Tick(float dt)
    {
        cd -= dt;
        skillCd -= dt;
    }

    public void Fire(AimContext aim)
    {
        if (!CanFire) return;
        cd = 1f / Mathf.Max(0.0001f, fireRate);
        OnFire(aim);
    }

    public void UseSkill(AimContext aim)
    {
        if (!CanUseSkill) return;

        skillCd = Mathf.Max(0f, skillCooldown);
        OnUseSkill(aim);
    }

    protected abstract void OnFire(AimContext aim);

    protected virtual void OnUseSkill(AimContext aim) { }

    // Для апгрейдов
    public void AddDamage(float add) => damage += add;
    public void MulDamage(float mul) => damage *= mul;
    public void AddFireRate(float add) => fireRate += add;
}
