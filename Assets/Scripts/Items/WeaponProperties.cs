using UnityEngine;

public class WeaponPropertiesHolder : MonoBehaviour
{

    public float Damage;
    public float RandomDamageOffsit;
    public bool CanGib;
    public WeaponTypes WeaponType;
    public float Cooldown;
    public float KnockBack;
    public float Recoil;
    public float StunLength;
    public int MaxHits;
    public ItemProperties Properties;

    private void OnValidate() {
        Properties = GetComponent<ItemProperties>();
    }

    private void Start() {
        Properties.Damage = Damage;
        Properties.KnockBack = KnockBack;
        Properties.AttackSpeed = 1.0f / Cooldown;
    }

    private void OnEnable() {
        if (Properties)Properties.Damage = Damage;
        if (Properties)Properties.KnockBack = KnockBack;
        if (Properties)Properties.AttackSpeed = 1.0f / Cooldown;
    }

    public enum WeaponTypes{
        ShortBlade,
        LongBlade,
        Blunt,
        FireArm,
    }
}
