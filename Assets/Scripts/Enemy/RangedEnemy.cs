using UnityEngine;

public sealed class RangedEnemy : Enemy
{
    [Header("Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float bulletDamage = 8f;
    [SerializeField] private float shootRate = 1.2f;
    [SerializeField] private float shootRange = 30f;
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float minDistance = 10f;
    [SerializeField] private float maxDistance = 20f;

    [Header("Aim")]
    [SerializeField] private float aimHeight = 1.2f;

    private float shootCd;

    protected override void Start()
    {
        base.Start();
        shootCd = Random.Range(0f, 0.5f);
    }

    protected override void TickAI(Vector3 toPlayerFlat, Vector3 separation)
    {
        shootCd -= Time.deltaTime;

        float dist = toPlayerFlat.magnitude;

        // держим дистанцию
        Vector3 seek = Vector3.zero;
        if (dist > maxDistance) seek = toPlayerFlat.normalized;
        else if (dist < minDistance) seek = -toPlayerFlat.normalized;

        Vector3 dir = seek + separation;
        if (dir.sqrMagnitude > 0.001f)
            desiredVelocity = dir.normalized * EffectiveMoveSpeed;

        // стрельба
        if (dist <= shootRange && shootCd <= 0f && projectilePrefab != null && shootPoint != null)
        {
            shootCd = 1f / Mathf.Max(0.0001f, shootRate);

            Vector3 targetPos = player.position + Vector3.up * aimHeight;
            Vector3 shootDir = (targetPos - shootPoint.position).normalized;

            GameObject go = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

            Projectile pr = go.GetComponent<Projectile>();
            if (pr != null)
                pr.Init(shootDir, projectileSpeed, bulletDamage, false, ProjectileTarget.Player);
        }
    }

    public override void ApplyWaveScaling(float hpMultiplier, float damageMultiplier)
    {
        base.ApplyWaveScaling(hpMultiplier, damageMultiplier);
        bulletDamage *= Mathf.Max(0f, damageMultiplier);
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        shootRate = Mathf.Max(0f, shootRate);
        shootRange = Mathf.Max(0f, shootRange);
        projectileSpeed = Mathf.Max(0f, projectileSpeed);
        bulletDamage = Mathf.Max(0f, bulletDamage);
        minDistance = Mathf.Max(0f, minDistance);
        maxDistance = Mathf.Max(minDistance, maxDistance);
        aimHeight = Mathf.Max(0f, aimHeight);
    }
}
