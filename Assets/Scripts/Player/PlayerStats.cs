using UnityEngine;
using System;
using System.Collections.Generic;

public sealed class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float hp = 100f;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Progression")]
    [SerializeField] private int level = 1;
    [SerializeField] private float currentExperience = 0f;
    [SerializeField] private float experienceToNextLevel = 50f;
    [SerializeField] private float experienceGrowthPerLevel = 1.25f;

    // --- Public read-only ---
    public float maxHpValue => maxHp;
    public float hpValue => hp;

    public float MaxHp => maxHp;
    public float Hp => hp;
    public float MoveSpeed => moveSpeed;
    public bool IsDead => hp <= 0f;
    public int Level => level;
    public float CurrentExperience => currentExperience;
    public float ExperienceToNextLevel => experienceToNextLevel;
    public float ExperienceProgress01 => experienceToNextLevel > 0f ? Mathf.Clamp01(currentExperience / experienceToNextLevel) : 1f;

    public int Level => level;
    public float Experience => experience;
    public float ExpToNextLevel => expToNextLevel;

    public event Action<int> OnLevelUp;

    // --- Upgrades tracking ---
    private readonly HashSet<string> takenUpgrades = new HashSet<string>();
    public bool HasUpgrade(string id) => !string.IsNullOrEmpty(id) && takenUpgrades.Contains(id);
    public bool TryTakeUpgrade(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return takenUpgrades.Add(id);
    }

    private void Awake()
    {
        maxHp = Mathf.Max(1f, maxHp);
        hp = Mathf.Clamp(hp, 0f, maxHp);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        level = Mathf.Max(1, level);
        experienceToNextLevel = Mathf.Max(1f, experienceToNextLevel);
        experienceGrowthPerLevel = Mathf.Max(1f, experienceGrowthPerLevel);
        currentExperience = Mathf.Clamp(currentExperience, 0f, experienceToNextLevel);
    }

    public void AddExperience(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        currentExperience += amount;

        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            LevelUp();
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        hp = Mathf.Max(0f, hp - amount);

        if (hp <= 0f)
        {
            GameManager.Instance?.PlayerDied();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        hp = Mathf.Min(maxHp, hp + amount);
    }

    public void AddExperience(float amount)
    {
        if (amount <= 0f) return;

        experience += amount;

        while (experience >= expToNextLevel)
        {
            experience -= expToNextLevel;
            level++;
            expToNextLevel = Mathf.Ceil(expToNextLevel * expGrowthPerLevel);
            OnLevelUp?.Invoke(level);
        }
    }

    // Апгрейды
    public void AddMaxHp(float add)
    {
        if (add <= 0f) return;
        maxHp += add;
        hp = Mathf.Min(hp + add, maxHp);
    }

    public void AddMoveSpeed(float add)
    {
        moveSpeed = Mathf.Max(0f, moveSpeed + add);
    }

    private void LevelUp()
    {
        level++;
        experienceToNextLevel = Mathf.Max(1f, experienceToNextLevel * experienceGrowthPerLevel);
    }
}
