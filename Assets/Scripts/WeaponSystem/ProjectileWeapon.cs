using UnityEngine;

public sealed class ProjectileWeapon : WeaponBase
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private bool pierce = false;

    [Header("Skill: Piercing Fan")]
    [SerializeField] private int fanProjectiles = 7;
    [SerializeField] private float fanAngle = 45f;
    [SerializeField] private float fanSpeedMultiplier = 1.15f;

    public void SetPierce(bool on) => pierce = on;
    public void MulProjectileSpeed(float mul) => projectileSpeed = Mathf.Max(0f, projectileSpeed * mul);

    private void Awake()
    {
        weaponName = "Gun";
    }

    protected override void OnFire(AimContext aim)
    {
        if (projectilePrefab == null || aim.muzzle == null) return;

        Vector3 dir = (aim.aimPoint - aim.muzzle.position).normalized;

        SpawnProjectile(aim.muzzle.position, dir, projectileSpeed, damage, pierce);
    }

    protected override void OnUseSkill(AimContext aim)
    {
        if (projectilePrefab == null || aim.muzzle == null) return;

        Vector3 baseDir = (aim.aimPoint - aim.muzzle.position).normalized;
        if (baseDir.sqrMagnitude < 0.0001f)
            baseDir = aim.owner != null ? aim.owner.forward : Vector3.forward;

        baseDir.y = 0f;
        if (baseDir.sqrMagnitude < 0.0001f)
            baseDir = Vector3.forward;
        baseDir.Normalize();

        int projectiles = Mathf.Max(1, fanProjectiles);
        float totalAngle = Mathf.Max(0f, fanAngle);

        if (projectiles == 1)
        {
            SpawnProjectile(aim.muzzle.position, baseDir, projectileSpeed * fanSpeedMultiplier, damage, true);
            return;
        }

        for (int i = 0; i < projectiles; i++)
        {
            float t = i / (projectiles - 1f);
            float yaw = Mathf.Lerp(-totalAngle * 0.5f, totalAngle * 0.5f, t);
            Vector3 dir = Quaternion.AngleAxis(yaw, Vector3.up) * baseDir;

            SpawnProjectile(aim.muzzle.position, dir, projectileSpeed * fanSpeedMultiplier, damage, true);
        }
    }

    private void SpawnProjectile(Vector3 position, Vector3 direction, float speed, float projectileDamage, bool isPiercing)
    {
        GameObject go = Instantiate(projectilePrefab, position, Quaternion.identity);
        Projectile pr = go.GetComponent<Projectile>();
        if (pr != null)
            pr.Init(direction, speed, projectileDamage, isPiercing, ProjectileTarget.Enemy);
    }
}
