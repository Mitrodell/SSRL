using UnityEngine;

public sealed class PiercingBeamWeapon : WeaponBase
{
    [Header("Beam")]
    [SerializeField] private float range = 60f;
    [SerializeField] private int maxHits = 6;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private bool stopOnWall = true;

    [Header("Skill: Slow Puddle")]
    [SerializeField] private float puddleRadius = 3f;
    [SerializeField] private float puddleDuration = 4f;
    [SerializeField] private float puddleSlowMultiplier = 0.45f;
    [SerializeField] private LayerMask puddleEnemyMask;

    private RaycastHit[] hits = new RaycastHit[32];

    private void Awake()
    {
        weaponName = "Laser";
    }

    protected override void OnFire(AimContext aim)
    {
        if (aim.muzzle == null) return;

        Vector3 start = aim.muzzle.position;
        Vector3 dir = (aim.aimPoint - start).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = aim.owner.forward;

        int count = Physics.RaycastNonAlloc(start, dir, hits, range, hitMask, QueryTriggerInteraction.Ignore);
        if (count <= 0) return;

        // сортируем по дистанции (RaycastNonAlloc может вернуть неупорядоченно)
        System.Array.Sort(hits, 0, count, new HitDistanceComparer());

        int damaged = 0;
        for (int i = 0; i < count; i++)
        {
            var h = hits[i];
            if (h.collider == null) continue;

            // если попали в стену и нужно остановиться
            if (stopOnWall && h.collider.GetComponentInParent<Enemy>() == null)
                break;

            Enemy e = h.collider.GetComponentInParent<Enemy>();
            if (e != null && !e.IsDead)
            {
                e.TakeDamage(damage);
                damaged++;
                if (damaged >= maxHits) break;
            }
        }
    }

    protected override void OnUseSkill(AimContext aim)
    {
        GameObject puddle = new GameObject("LaserSlowPuddle");
        puddle.transform.position = aim.aimPoint;

        SlowPuddle slow = puddle.AddComponent<SlowPuddle>();
        slow.Init(puddleRadius, puddleDuration, puddleSlowMultiplier, puddleEnemyMask);
    }

    public void MulRange(float mul) => range = Mathf.Max(1f, range * mul);
    public void AddMaxHits(int add) => maxHits = Mathf.Max(1, maxHits + add);

    private sealed class HitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
