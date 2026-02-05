public interface IWeapon
{
    string WeaponName { get; }
    bool CanFire { get; }
    bool CanUseSkill { get; }
    void Tick(float dt);
    void Fire(AimContext aim);
    void UseSkill(AimContext aim);
}
