using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxHp = 30f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float touchDamage = 10f;
    [SerializeField] private float experienceDrop = 10f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 540f;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 1.2f;
    [SerializeField] private float separationStrength = 2.2f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = false;

    [SerializeField] private float currentHp;

    protected Transform player;
    protected Rigidbody rb;

    private readonly Collider[] sepHits = new Collider[16];
    private float slowMultiplier = 1f;
    private float slowTimer;

    // movement “intent” computed in Update, applied in FixedUpdate
    protected Vector3 desiredVelocity;
    protected Quaternion desiredRotation;
    protected bool hasRotation;

    // --- Public API ---
    public float MaxHp => maxHp;
    public float CurrentHp => currentHp;
    public float MoveSpeed => moveSpeed;
    public float EffectiveMoveSpeed => moveSpeed * slowMultiplier;
    public float TouchDamage => touchDamage;
    public float ExperienceDrop => experienceDrop;
    public bool IsDead => currentHp <= 0f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (currentHp <= 0f) currentHp = maxHp;
    }

    protected virtual void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        currentHp = maxHp;
    }

    protected virtual void Update()
    {
        desiredVelocity = Vector3.zero;
        hasRotation = false;

        TickSlow();

        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        if (player == null) return;
        if (IsDead) return;

        Vector3 to = player.position - transform.position;
        to.y = 0f;

        if (to.sqrMagnitude > 0.01f)
        {
            desiredRotation = Quaternion.LookRotation(to.normalized, Vector3.up);
            hasRotation = true;
        }

        Vector3 sep = ComputeSeparation();
        TickAI(to, sep); // <-- поведение конкретного врага
    }

    protected virtual void FixedUpdate()
    {
        if (IsDead) return;

        if (desiredVelocity.sqrMagnitude > 0.00001f)
            rb.MovePosition(rb.position + desiredVelocity * Time.fixedDeltaTime);

        if (hasRotation)
        {
            float maxStep = rotationSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, desiredRotation, maxStep));
        }
    }

    /// <summary>Логика конкретного врага: задать desiredVelocity и (опционально) стрелять.</summary>
    protected abstract void TickAI(Vector3 toPlayerFlat, Vector3 separation);

    protected Vector3 ComputeSeparation()
    {
        if (separationRadius <= 0f) return Vector3.zero;

        int count = Physics.OverlapSphereNonAlloc(transform.position, separationRadius, sepHits, enemyMask);
        if (count <= 1) return Vector3.zero;

        Vector3 push = Vector3.zero;
        int n = 0;

        for (int i = 0; i < count; i++)
        {
            Collider c = sepHits[i];
            if (c == null) continue;

            // Важно: если DamageTrigger попадает в маску, лучше выставить ему другой слой.
            Transform t = c.transform.root; // root снижает “двойной вес” дочерних коллайдеров
            if (t == transform) continue;

            Vector3 away = transform.position - t.position;
            away.y = 0f;

            float d2 = away.sqrMagnitude;
            if (d2 < 0.0001f) continue;

            push += away.normalized / d2;
            n++;
        }

        if (n == 0) return Vector3.zero;
        return (push / n) * separationStrength;
    }

    public void TakeDamage(float dmg)
    {
        if (IsDead) return;
        if (dmg <= 0f) return;

        currentHp = Mathf.Max(0f, currentHp - dmg);

        if (currentHp <= 0f)
        {
            GameManager.Instance?.EnemyKilled(this);
            Destroy(gameObject);
        }
    }

    public void ApplySlow(float speedMultiplier, float duration)
    {
        float clampedMultiplier = Mathf.Clamp(speedMultiplier, 0.05f, 1f);
        slowMultiplier = Mathf.Min(slowMultiplier, clampedMultiplier);
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    public virtual void ApplyWaveScaling(float hpMultiplier, float damageMultiplier)
    {
        maxHp *= Mathf.Max(0.01f, hpMultiplier);
        currentHp = maxHp;

        touchDamage *= Mathf.Max(0f, damageMultiplier);
    }

    protected virtual void OnValidate()
    {
        maxHp = Mathf.Max(1f, maxHp);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        touchDamage = Mathf.Max(0f, touchDamage);
        experienceDrop = Mathf.Max(0f, experienceDrop);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        separationRadius = Mathf.Max(0f, separationRadius);
        separationStrength = Mathf.Max(0f, separationStrength);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }

    private void TickSlow()
    {
        if (slowTimer <= 0f)
        {
            slowMultiplier = 1f;
            return;
        }

        slowTimer -= Time.deltaTime;
        if (slowTimer <= 0f)
            slowMultiplier = 1f;
    }
}
