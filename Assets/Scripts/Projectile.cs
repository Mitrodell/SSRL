using UnityEngine;

public enum ProjectileTarget
{
    Enemy,
    Player
}

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 18f;
    public float damage = 10f;
    public bool pierce = false;
    public float life = 3f;

    [Header("Targeting")]
    public ProjectileTarget target = ProjectileTarget.Enemy;

    private Vector3 direction = Vector3.forward;

    public void Init(Vector3 dir, float spd, float dmg, bool prc, ProjectileTarget tgt)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        pierce = prc;
        target = tgt;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;
        transform.position += direction * speed * Time.deltaTime;

        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (target == ProjectileTarget.Enemy)
        {
            Enemy e = other.GetComponent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(damage);
                if (!pierce) Destroy(gameObject);
            }
        }
        else
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(damage);
                if (!pierce) Destroy(gameObject);
            }
        }
    }
}
