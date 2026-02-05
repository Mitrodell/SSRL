using UnityEngine;
using TMPro;
using UnityEngine.UI;

public sealed class HUDController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStats player;
    [SerializeField] private WeaponSystem weaponSystem;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI weaponText;

    [Header("UI - HP Bar")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private bool smoothBar = true;
    [SerializeField] private float barLerpSpeed = 12f;

    private float lastHp = int.MinValue;
    private float lastMaxHp = int.MinValue;
    private int lastWave = int.MinValue;
    private string lastWeaponName = null;
    private float shownFill = 1f;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerStats>();
        if (weaponSystem == null) weaponSystem = FindFirstObjectByType<WeaponSystem>();
        if (hpFillImage != null)
            shownFill = hpFillImage.fillAmount;
    }

    private void Update()
    {
        UpdateHP();
        UpdateWave();
        UpdateWeapon();
    }

    private void UpdateHP()
    {
        if (player == null)
        {
            if (hpText != null) hpText.text = "HP: -/-";
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

        if (hpFillImage != null)
        {
            float target = lastHp / lastMaxHp;

            if (smoothBar)
            {
                shownFill = Mathf.Lerp(shownFill, target, barLerpSpeed * Time.deltaTime);
                hpFillImage.fillAmount = shownFill;
            }
            else
            {
                shownFill = target;
                hpFillImage.fillAmount = shownFill;
            }
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
