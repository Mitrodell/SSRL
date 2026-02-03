public interface IWeapon
{
    string WeaponName { get; }
    bool CanFire { get; }
    void Tick(float dt);
    void Fire(AimContext aim);
}
