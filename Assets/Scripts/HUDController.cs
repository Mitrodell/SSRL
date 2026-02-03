using UnityEngine;
using TMPro;

public sealed class HUDController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStats player;
    [SerializeField] private WeaponSystem weaponSystem;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI weaponText;

    private int lastHp = int.MinValue;
    private int lastMaxHp = int.MinValue;
    private int lastWave = int.MinValue;
    private string lastWeaponName = null;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerStats>();
        if (weaponSystem == null) weaponSystem = FindFirstObjectByType<WeaponSystem>();
    }

    private void Update()
    {
        UpdateHP();
        UpdateWave();
        UpdateWeapon();
    }

    private void UpdateHP()
    {
        if (hpText == null) return;
        if (player == null)
        {
            hpText.text = "HP: -/-";
            return;
        }

        int hp = Mathf.CeilToInt(player.Hp);
        int maxHp = Mathf.CeilToInt(player.MaxHp);

        if (hp != lastHp || maxHp != lastMaxHp)
        {
            lastHp = hp;
            lastMaxHp = maxHp;
            hpText.text = $"HP: {hp}/{maxHp}";
        }
    }

    private void UpdateWave()
    {
        if (waveText == null) return;

        int wave = 0;
        if (GameManager.Instance != null)
            wave = GameManager.Instance.CurrentWave;

        if (wave != lastWave)
        {
            lastWave = wave;
            waveText.text = $"Wave {wave}";
        }
    }

    private void UpdateWeapon()
    {
        if (weaponText == null) return;

        string name = "-";
        if (weaponSystem != null && weaponSystem.Current != null)
            name = weaponSystem.Current.WeaponName;

        if (name != lastWeaponName)
        {
            lastWeaponName = name;
            weaponText.text = name;
        }
    }
}
