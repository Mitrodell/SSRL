using UnityEngine;

public sealed class PiercingBeamWeapon : WeaponBase
{
    [Header("Beam")]
    [SerializeField] private float range = 60f;
    [SerializeField] private int maxHits = 6;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private bool stopOnWall = true;
    [SerializeField] private LaserBeamVFX beamVfx;

    [Header("Skill: Slow Puddle")]
    [SerializeField] private GameObject puddlePrefab;
    [SerializeField] private float puddleRadius = 3f;
    [SerializeField] private float puddleDuration = 4f;
    [SerializeField, Range(0.05f, 1f)] private float puddleSlowMultiplier = 0.5f;
    [SerializeField] private LayerMask puddleEnemyMask;

    private RaycastHit[] hits = new RaycastHit[32];
    private float vfxHoldTimer;
    protected override bool UseFireCooldown => false;

    private void Awake()
    {
        weaponName = "Laser";
    }
    private void Update()
    {
        if (beamVfx == null) return;

        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
        {
            beamVfx.SetActive(false);
            return;
        }

        vfxHoldTimer -= Time.deltaTime;
        if (vfxHoldTimer <= 0f)
            beamVfx.SetActive(false);
    }
    protected override void OnFire(AimContext aim)
    {
        if (aim.muzzle == null) return;

        if (beamVfx != null) beamVfx.SetActive(true);

        Vector3 start = aim.muzzle.position;

        Vector3 dir = (aim.aimPoint - start);
        if (dir.sqrMagnitude < 0.0001f) dir = aim.owner.forward;
        dir.Normalize();

        Vector3 end = start + dir * range;

        int count = Physics.RaycastNonAlloc(start, dir, hits, range, hitMask, QueryTriggerInteraction.Ignore);
        bool hasImpact = false;
        if (count > 0)
        {
            System.Array.Sort(hits, 0, count, new HitDistanceComparer());

            if (stopOnWall)
            {
                for (int i = 0; i < count; i++)
                {
                    var h = hits[i];
                    if (h.collider == null) continue;

                    Enemy maybeEnemy = h.collider.GetComponentInParent<Enemy>();
                    if (maybeEnemy == null)
                    {
                        end = h.point;
                        hasImpact = true;
                        break;
                    }
                }
            }

            int damaged = 0;
            float dmgThisFrame = damage * Time.deltaTime;

            for (int i = 0; i < count; i++)
            {
                var h = hits[i];
                if (h.collider == null) continue;

                if (stopOnWall && h.collider.GetComponentInParent<Enemy>() == null)
                    break;

                Enemy e = h.collider.GetComponentInParent<Enemy>();
                if (e != null && !e.IsDead)
                {
                    e.TakeDamage(dmgThisFrame);
                    damaged++;
                    if (damaged >= maxHits) break;
                }
            }
        }
        if (beamVfx != null) beamVfx.SetBeam(start, end, hasImpact);
        vfxHoldTimer = 0.08f;
    }

    protected override void OnUseSkill(AimContext aim)
    {
        Vector3 spawnPosition = aim.aimPoint;

        SlowPuddle puddle = null;
        if (puddlePrefab != null)
        {
            GameObject go = Instantiate(puddlePrefab, spawnPosition, Quaternion.identity);
            puddle = go.GetComponent<SlowPuddle>();
        }
        else
        {
            var runtimePuddle = new GameObject("SlowPuddle");
            runtimePuddle.transform.position = spawnPosition;
            puddle = runtimePuddle.AddComponent<SlowPuddle>();
        }

        if (puddle != null)
            puddle.Init(puddleRadius, puddleDuration, puddleSlowMultiplier, puddleEnemyMask);
    }

    public void MulRange(float mul) => range = Mathf.Max(1f, range * mul);
    public void AddMaxHits(int add) => maxHits = Mathf.Max(1, maxHits + add);

    private sealed class HitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
