using System;
using UnityEngine;

public sealed class UpgradeChoice
{
    public string id;
    public string title;
    public Action<PlayerStats> apply;
    public bool oneTime;
    public string iconResourcePath;

    private Sprite cachedIcon;

    public Sprite Icon
    {
        get
        {
            if (cachedIcon != null || string.IsNullOrWhiteSpace(iconResourcePath))
                return cachedIcon;

            cachedIcon = Resources.Load<Sprite>(iconResourcePath);
            return cachedIcon;
        }
    }
}