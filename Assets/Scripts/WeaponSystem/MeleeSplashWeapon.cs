using UnityEngine;

public sealed class MeleeSplashWeapon : WeaponBase
{
    [Header("Melee")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float angle = 90f;      // сектор перед игроком
    [SerializeField] private LayerMask enemyMask;

    private readonly Collider[] hits = new Collider[32];


    protected override void OnFire(AimContext aim)
    {
        Vector3 origin = aim.owner.position + Vector3.up * 1.0f;
        int count = Physics.OverlapSphereNonAlloc(origin, radius, hits, enemyMask, QueryTriggerInteraction.Ignore);

        Vector3 forward = aim.owner.forward;
        forward.y = 0f;
        forward.Normalize();

        for (int i = 0; i < count; i++)
        {
            var c = hits[i];
            if (c == null) continue;

            Enemy e = c.GetComponentInParent<Enemy>();
            if (e == null || e.IsDead) continue;

            Vector3 to = e.transform.position - aim.owner.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.0001f) continue;

            float a = Vector3.Angle(forward, to.normalized);
            if (a <= angle * 0.5f)
                e.TakeDamage(damage);
        }
    }
    public void AddRadius(float add) => radius = Mathf.Max(0f, radius + add);
    public void AddAngle(float add) => angle = Mathf.Clamp(angle + add, 10f, 240f);

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.0f, radius);
    }
#endif
}
