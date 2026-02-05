using UnityEngine;
using System.Collections;

public sealed class MeleeSplashWeapon : WeaponBase
{
    [Header("Melee")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float angle = 90f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Skill: Leap")]
    [SerializeField] private float leapDistance = 20f;
    [SerializeField] private float leapDuration = 0.35f;
    [SerializeField] private float leapHeight = 2.5f;
    [SerializeField] private AnimationCurve leapArc = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f)
    );
    [SerializeField] private LayerMask leapBlockMask = ~0;

    private readonly Collider[] hits = new Collider[32];
    private Coroutine leapRoutine;

    private void Awake()
    {
        weaponName = "Sword";
    }

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

    protected override void OnUseSkill(AimContext aim)
    {
        if (aim.owner == null) return;

        Vector3 forward = aim.owner.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) return;
        forward.Normalize();

        Vector3 start = aim.owner.position + Vector3.up * 0.6f;
        float travel = leapDistance;

        if (Physics.Raycast(start, forward, out RaycastHit hit, leapDistance, leapBlockMask, QueryTriggerInteraction.Ignore))
            travel = Mathf.Max(0f, hit.distance - 0.5f);

        Vector3 target = aim.owner.position + forward * travel;

        if (leapRoutine != null)
            StopCoroutine(leapRoutine);

        leapRoutine = StartCoroutine(PerformLeap(aim.owner, target));
    }

    private IEnumerator PerformLeap(Transform owner, Vector3 target)
    {
        Vector3 start = owner.position;
        float duration = Mathf.Max(0.01f, leapDuration);

        CharacterController cc = owner.GetComponent<CharacterController>();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 horizontal = Vector3.Lerp(start, target, t);
            float verticalOffset = leapArc.Evaluate(t) * leapHeight;
            Vector3 desired = horizontal + Vector3.up * verticalOffset;

            if (cc != null)
                cc.Move(desired - owner.position);
            else
                owner.position = desired;

            yield return null;
        }

        if (cc != null)
            cc.Move(target - owner.position);
        else
            owner.position = target;

        leapRoutine = null;
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
