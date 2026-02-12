using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 2f;
    [SerializeField] protected float skillCooldown = 8f;
    protected string weaponName = "Weapon";

    protected float shootCooldown;
    protected float currentSkillCooldown;

    public string WeaponName => weaponName;
    public bool CanFire => shootCooldown <= 0f;
    public bool CanUseSkill => currentSkillCooldown <= 0f;
    public float FireRate => fireRate;

    public virtual void Tick(float dt)
    {
        shootCooldown -= dt;
        currentSkillCooldown -= dt;
    }

    public void Fire(AimContext aim)
    {
        if (UseFireCooldown && !CanFire) return;
        if (UseFireCooldown)
            shootCooldown = 1f / Mathf.Max(0.0001f, fireRate);
        OnFire(aim);
    }

    public void UseSkill(AimContext aim)
    {
        if (!CanUseSkill) return;

        currentSkillCooldown = Mathf.Max(0f, skillCooldown);
        OnUseSkill(aim);
    }
    protected virtual bool UseFireCooldown => true;
    protected abstract void OnFire(AimContext aim);

    protected virtual void OnUseSkill(AimContext aim) { }

    public void AddDamage(float add) => damage += add;
    public void MulDamage(float mul) => damage *= mul;
    public void AddFireRate(float add) => fireRate += add;
}
