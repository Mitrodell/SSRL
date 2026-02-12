using UnityEngine;
using System.Collections.Generic;

public static class UpgradeSystem
{
    private static readonly List<UpgradeChoice> all = new List<UpgradeChoice>
    {
        new UpgradeChoice {
            id = "hp_max_20",
            title = "+20 Max HP",
            apply = (ps) => ps.AddMaxHp(20f),
            oneTime=false
        },
        new UpgradeChoice {
            id = "heal_25",
            title = "Heal +25",
            apply = (ps) => ps.Heal(25f),
            oneTime=false
        },
        new UpgradeChoice {
            id = "move_speed_10p",
            title = "+10% Move Speed",
            apply = (ps) => ps.AddMoveSpeed(ps.MoveSpeed * 0.10f),
            oneTime=false
        },

        // Global weapon
        new UpgradeChoice {
            id = "weapon_damage_20p",
            title = "+20% Weapon Damage",
            apply = (ps) => WithCurrentWeapon(ps, w => w.MulDamage(1.20f)),
            oneTime=false
        },
        new UpgradeChoice {
            id = "weapon_firerate_15p",
            title = "+15% Fire Rate",
            apply = (ps) => WithCurrentWeapon(ps, w => w.AddFireRate(w.FireRate * 0.15f)),
            oneTime=false
        },

        // Melee weapon
        new UpgradeChoice {
            id = "melee_radius_up",
            title = "Melee: +0.4 Radius",
            apply = (ps) => WithWeapon<MeleeSplashWeapon>(ps, w => w.AddRadius(0.4f)),
            oneTime=false
        },
        new UpgradeChoice {
            id = "melee_angle_up",
            title = "Melee: +20° Arc",
            apply = (ps) => WithWeapon<MeleeSplashWeapon>(ps, w => w.AddAngle(20f)),
            oneTime=false
        },
        new UpgradeChoice {
            id = "melee_damage_25p",
            title = "Melee: +25% Damage",
            apply = (ps) => WithWeapon<MeleeSplashWeapon>(ps, w => w.MulDamage(1.25f)),
            oneTime=false,
        },

        // Gun weapon
        new UpgradeChoice {
            id = "gun_proj_speed_up",
            title = "Gun: +20% Bullet Speed",
            apply = (ps) => WithWeapon<ProjectileWeapon>(ps, w => w.MulProjectileSpeed(1.20f)),
            oneTime=false
        },
        new UpgradeChoice {
            id = "gun_pierce_on",
            title = "Gun: Piercing Shots",
            apply = (ps) => WithWeapon<ProjectileWeapon>(ps, w => w.SetPierce(true)),
            oneTime=true
        },

        //Beam weapon
        new UpgradeChoice {
            id = "beam_range_up",
            title = "Beam: +25% Range",
            apply = (ps) => WithWeapon<PiercingBeamWeapon>(ps, w => w.MulRange(1.25f)),
            oneTime=false
        },
        new UpgradeChoice {
            id = "beam_hits_up",
            title = "Beam: +2 Targets",
            apply = (ps) => WithWeapon<PiercingBeamWeapon>(ps, w => w.AddMaxHits(2)),
            oneTime=false
        },
        new UpgradeChoice {
            id = "beam_damage_25p",
            title = "Beam: +25% Damage",
            apply = (ps) => WithWeapon<PiercingBeamWeapon>(ps, w => w.MulDamage(1.25f)),
            oneTime=false
        },
    };

    public static UpgradeChoice RandomChoice(PlayerStats ps)
    {
        List<UpgradeChoice> available = new List<UpgradeChoice>(all.Count);
        for (int i = 0; i < all.Count; i++)
        {
            var u = all[i];
            if (u.oneTime && ps != null && ps.HasUpgrade(u.id)) continue;
            available.Add(u);
        }

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }

    private static void WithCurrentWeapon(PlayerStats ps, System.Action<WeaponBase> act)
    {
        if (ps == null) return;

        WeaponSystem ws = ps.GetComponent<WeaponSystem>();
        if (ws == null || ws.CurrentWeapon == null) return;

        act(ws.CurrentWeapon);
    }

    private static void WithWeapon<T>(PlayerStats ps, System.Action<T> act) where T : WeaponBase
    {
        if (ps == null) return;
        T w = ps.GetComponent<T>();
        if (w == null) w = ps.GetComponentInChildren<T>();
        if (w == null) return;
        act(w);
    }
}
