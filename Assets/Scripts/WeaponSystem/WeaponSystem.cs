using UnityEngine;
using UnityEngine.InputSystem;

public sealed class WeaponSystem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform muzzle;
    [SerializeField] private LayerMask aimMask;
    [SerializeField] private float aimMaxDistance = 120f;

    [Header("Weapons")]
    [SerializeField] private WeaponBase meleeWeapon;
    [SerializeField] private WeaponBase gunWeapon;
    [SerializeField] private WeaponBase beamWeapon;

    [Header("Input")]
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference skillAction;
    [SerializeField] private InputActionReference selectMeleeAction;
    [SerializeField] private InputActionReference selectGunAction;
    [SerializeField] private InputActionReference selectBeamAction;

    private WeaponBase currentWeapon;
    private PlayerStats stats;
    private readonly System.Collections.Generic.List<WeaponBase> weapons = new System.Collections.Generic.List<WeaponBase>(3);
    private int currentIndex;

    public WeaponBase CurrentWeapon => currentWeapon;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        stats = GetComponent<PlayerStats>();

        BuildWeaponList();
        if (weapons.Count > 0)
        {
            currentIndex = Mathf.Clamp(currentIndex, 0, weapons.Count - 1);
            currentWeapon = weapons[currentIndex];
        }
        else
        {
            Debug.LogWarning("[WeaponSystem] No weapons assigned.");
        }
    }

    private void OnEnable()
    {
        attackAction?.action?.Enable();
        skillAction?.action?.Enable();
        selectMeleeAction?.action?.Enable();
        selectGunAction?.action?.Enable();
        selectBeamAction?.action?.Enable();
    }

    private void OnDisable()
    {
        attackAction?.action?.Disable();
        skillAction?.action?.Disable();
        selectMeleeAction?.action?.Disable();
        selectGunAction?.action?.Disable();
        selectBeamAction?.action?.Disable();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        if (stats != null && stats.IsDead)
            return;

        if (currentWeapon == null)
            return;

        float dt = Time.deltaTime;
        currentWeapon.Tick(dt);

        if (WasPressed(selectMeleeAction)) Equip(meleeWeapon);
        if (WasPressed(selectGunAction)) Equip(gunWeapon);
        if (WasPressed(selectBeamAction)) Equip(beamWeapon);

        AimContext aim = BuildAimContext();

        if (WasPressed(skillAction))
            currentWeapon.UseSkill(aim);

        if (IsPressed(attackAction))
            currentWeapon.Fire(aim);
    }

    public void Equip(WeaponBase weapon)
    {
        if (weapon == null) return;
        if (weapon == currentWeapon) return;

        currentWeapon = weapon;
        int index = weapons.IndexOf(currentWeapon);
        if (index >= 0) currentIndex = index;
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

    private void BuildWeaponList()
    {
        weapons.Clear();
        if (meleeWeapon != null) weapons.Add(meleeWeapon);
        if (gunWeapon != null) weapons.Add(gunWeapon);
        if (beamWeapon != null) weapons.Add(beamWeapon);
    }

    private static bool WasPressed(InputActionReference action)
    {
        return action != null && action.action != null && action.action.WasPressedThisFrame();
    }

    private static bool IsPressed(InputActionReference action)
    {
        return action != null && action.action != null && action.action.IsPressed();
    }
}
