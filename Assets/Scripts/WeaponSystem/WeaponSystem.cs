using UnityEngine;

public sealed class WeaponSystem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform muzzle;        // shootPoint игрока
    [SerializeField] private LayerMask aimMask;       // Enemy + Ground + Default
    [SerializeField] private float aimMaxDistance = 120f;

    [Header("Weapons (components on this object or children)")]
    [SerializeField] private WeaponBase meleeWeapon;
    [SerializeField] private WeaponBase gunWeapon;
    [SerializeField] private WeaponBase beamWeapon;

    private WeaponBase current;
    private PlayerStats stats;

    public WeaponBase Current => current;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        stats = GetComponent<PlayerStats>();

        // Выбор оружия по умолчанию
        if (gunWeapon != null) current = gunWeapon;
        else if (meleeWeapon != null) current = meleeWeapon;
        else if (beamWeapon != null) current = beamWeapon;
        else
            Debug.LogWarning("[WeaponSystem] No weapons assigned.");
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        if (stats != null && stats.IsDead)
            return;

        if (current == null)
            return;

        float dt = Time.deltaTime;
        current.Tick(dt);

        // Переключение оружия
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(meleeWeapon);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Equip(gunWeapon);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Equip(beamWeapon);

        // Огонь
        if (Input.GetMouseButton(0))
        {
            AimContext aim = BuildAimContext();
            current.Fire(aim);
        }
    }

    public void Equip(WeaponBase weapon)
    {
        if (weapon == null) return;
        if (weapon == current) return;

        current = weapon;
    }

    private AimContext BuildAimContext()
    {
        return new AimContext
        {
            owner = transform,
            muzzle = muzzle,
            cam = cam,
            aimPoint = GetAimPoint()
        };
    }

    private Vector3 GetAimPoint()
    {
        if (cam == null)
            return transform.position + transform.forward * 50f;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, aimMaxDistance, aimMask, QueryTriggerInteraction.Ignore))
            return hit.point;

        return ray.origin + ray.direction * aimMaxDistance;
    }
}
