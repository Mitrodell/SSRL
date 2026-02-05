using UnityEngine;

public sealed class SlowPuddle : MonoBehaviour
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private float duration = 4f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private LayerMask enemyMask;

    private readonly Collider[] hits = new Collider[64];

    public void Init(float puddleRadius, float puddleDuration, float enemySpeedMultiplier, LayerMask mask)
    {
        radius = Mathf.Max(0.1f, puddleRadius);
        duration = Mathf.Max(0.1f, puddleDuration);
        slowMultiplier = Mathf.Clamp(enemySpeedMultiplier, 0.05f, 1f);
        enemyMask = mask;
    }

    private void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, enemyMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            Collider c = hits[i];
            if (c == null) continue;

            Enemy enemy = c.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.IsDead)
                enemy.ApplySlow(slowMultiplier, Time.deltaTime + 0.15f);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
