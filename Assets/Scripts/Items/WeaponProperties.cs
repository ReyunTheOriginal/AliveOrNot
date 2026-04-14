using System.Collections.Generic;
using UnityEngine;

public class WeaponPropertiesHolder : MonoBehaviour
{

    public float Damage;
    public float RandomDamageOffsit;
    public bool CanGib;
    public WeaponTypes WeaponType;
    public float Cooldown;
    public float KnockBack;
    public int MaxHits;

    public enum WeaponTypes{
        ShortBlade,
        LongBlade,
        Blunt,
        FireArm,
    }
}
