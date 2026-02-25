using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

public sealed class HUDController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStats player;
    [SerializeField] private WeaponSystem weaponSystem;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;

    [Header("UI - HP Bar")]
    [SerializeField] private Image hpFillImage;

    [Header("UI - Experience Bar")]
    [SerializeField] private Image expFillImage;
    [SerializeField] private bool smoothBar = true;
    [SerializeField] private float barLerpSpeed = 12f;

    [Header("UI - Weapons")]
    [SerializeField] private Image meleeImage;
    [SerializeField] private Image gunImage;
    [SerializeField] private Image laserImage;

    private float lastHp = int.MinValue;
    private float lastMaxHp = int.MinValue;
    private int lastWave = int.MinValue;
    private int lastLevel = int.MinValue;
    private float lastExp = int.MinValue;
    private float lastExpToNext = int.MinValue;
    private float shownHpFill = 1f;
    private float shownExpFill = 0f;
    private int lastWeaponIndex = 1;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerStats>();
        if (weaponSystem == null) weaponSystem = FindFirstObjectByType<WeaponSystem>();
        if (hpFillImage != null)
            shownHpFill = hpFillImage.fillAmount;
        if (expFillImage != null)
            shownExpFill = expFillImage.fillAmount;
    }

    private void Update()
    {
        UpdateHP();
        UpdateWave();
        UpdateWeapon();
        UpdateProgression();
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
                shownHpFill = Mathf.Lerp(shownHpFill, target, barLerpSpeed * Time.deltaTime);
                hpFillImage.fillAmount = shownHpFill;
            }
            else
            {
                shownHpFill = target;
                hpFillImage.fillAmount = shownHpFill;
            }
        }
    }


    private void UpdateProgression()
    {
        if (player == null)
        {
            if (levelText != null) levelText.text = "-";
            if (experienceText != null) experienceText.text = "EXP -/-";
            return;
        }

        int level = player.Level;
        int exp = Mathf.FloorToInt(player.CurrentExperience);
        int expToNext = Mathf.CeilToInt(player.ExperienceToNextLevel);

        if (levelText != null && level != lastLevel)
        {
            lastLevel = level;
            levelText.text = $"{level}";
        }

        if (experienceText != null && (exp != lastExp || expToNext != lastExpToNext))
        {
            lastExp = exp;
            lastExpToNext = expToNext;
            experienceText.text = $"EXP: {exp}/{expToNext}";
        }

        if (expFillImage != null)
        {
            float target = lastExp/lastExpToNext;
            if (smoothBar)
            {
                shownExpFill = Mathf.Lerp(shownExpFill, target, barLerpSpeed * Time.deltaTime);
                expFillImage.fillAmount = shownExpFill;
            }
            else
            {
                shownExpFill = target;
                expFillImage.fillAmount = shownExpFill;
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
        if (weaponSystem == null) return;
        if (lastWeaponIndex == weaponSystem.CurrentIndex) return;
        lastWeaponIndex = weaponSystem.CurrentIndex;

        Color c = meleeImage.color;
        c.a = 0.5f;
        switch (lastWeaponIndex)
        {
            case 0:
                c.a = 1f;
                meleeImage.color = c;
                c.a = 0.5f;
                gunImage.color = c;
                laserImage.color = c;
                break;
            case 1:
                c.a = 1f;
                gunImage.color = c;
                c.a = 0.5f;
                meleeImage.color = c;
                laserImage.color = c;
                break;
            case 2:
                c.a = 1f;
                laserImage.color = c;
                c.a = 0.5f;
                gunImage.color = c;
                meleeImage.color = c;
                break;
        }
    }
}
