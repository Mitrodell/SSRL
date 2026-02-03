using System;

public sealed class UpgradeChoice
{
    public string id;
    public string title;
    public Action<PlayerStats> apply;
    public bool oneTime;
}
