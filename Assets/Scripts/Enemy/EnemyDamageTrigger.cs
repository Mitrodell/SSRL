using UnityEngine;

public sealed class EnemyDamageTrigger : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private float hitInterval = 0.6f;

    private float hitCd;

    private void Awake()
    {
        if (enemy == null) enemy = GetComponentInParent<Enemy>();
        if (enemy == null) Debug.LogError("[EnemyDamageTrigger] Enemy not found in parent.");
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        hitCd -= Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (enemy == null) return;
        if (enemy.IsDead) return;

        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        if (hitCd > 0f) return;

        if (!other.CompareTag("Player")) return;

        PlayerStats ps = other.GetComponent<PlayerStats>();
        if (ps == null) return;

        ps.TakeDamage(enemy.TouchDamage);
        hitCd = hitInterval;
    }
}
