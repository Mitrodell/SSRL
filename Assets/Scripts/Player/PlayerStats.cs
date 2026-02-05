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
    [SerializeField] private float currentExperience;
    [SerializeField] private float experienceToNextLevel = 5f;
    [SerializeField] private float experienceGrowthPerLevel = 1.35f;

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
        currentExperience = Mathf.Max(0f, currentExperience);
        experienceToNextLevel = Mathf.Max(1f, experienceToNextLevel);
        experienceGrowthPerLevel = Mathf.Max(1.05f, experienceGrowthPerLevel);
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

        currentExperience += amount;

        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            level++;
            experienceToNextLevel = Mathf.Ceil(experienceToNextLevel * experienceGrowthPerLevel);
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
}
