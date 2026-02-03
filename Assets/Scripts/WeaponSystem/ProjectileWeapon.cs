using UnityEngine;

public sealed class ProjectileWeapon : WeaponBase
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private bool pierce = false; // <-- ДОБАВИЛИ

    public void SetPierce(bool on) => pierce = on;
    public void MulProjectileSpeed(float mul) => projectileSpeed = Mathf.Max(0f, projectileSpeed * mul);

    protected override void OnFire(AimContext aim)
    {
        if (projectilePrefab == null || aim.muzzle == null) return;

        Vector3 dir = (aim.aimPoint - aim.muzzle.position).normalized;

        GameObject go = Instantiate(projectilePrefab, aim.muzzle.position, Quaternion.identity);
        Projectile pr = go.GetComponent<Projectile>();
        if (pr != null)
            pr.Init(dir, projectileSpeed, damage, pierce, ProjectileTarget.Enemy);
    }
}
