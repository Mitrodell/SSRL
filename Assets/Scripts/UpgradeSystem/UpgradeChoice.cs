using System;
using System.Reflection;
using UnityEngine;

public sealed class UpgradeChoice
{
    public string id;
    public string title;
    public Action<PlayerStats> apply;
    public bool oneTime;
    public string iconPath;
    private Sprite _iconCache;
    public Sprite Icon
    {
        get
        {
            if (_iconCache != null) return _iconCache;
            if (string.IsNullOrEmpty(iconPath)) return null;
            _iconCache = Resources.Load<Sprite>(iconPath);
            return _iconCache;
        }
    }
}