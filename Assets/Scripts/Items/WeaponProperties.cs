using UnityEngine;

public class WeaponPropertiesHolder : MonoBehaviour
{
    public float Damage;
    public float RandomDamageOffsit;
    public EnemyProperties.DeathTypes KillType;
    public WeaponTypes WeaponType;
    public float Cooldown;
    public float KnockBack;

    public enum WeaponTypes{
        ShortBlade,
        LongBlade,
        Blunt,
        FireArm,
    }
}
