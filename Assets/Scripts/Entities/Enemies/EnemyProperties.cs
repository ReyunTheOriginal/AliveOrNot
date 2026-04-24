using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProperties : MonoBehaviour{
    public float MaxHealth;
    [SerializeField]private float Health ;
    public float WalkSpeed;
    
    public Dictionary<State, bool> CurrentStates = new Dictionary<State, bool>{
        {State.Chasing, false},
        {State.Roaming, false},
        {State.Idle, false},
        {State.Attacking, false},
        {State.Spawning, false},
    };

    public Direction CurrentDirection;
    public Dictionary<Effects, float> CurrentEffects = new Dictionary<Effects, float>();
    public GameObject BloodBurst;
[Header("DeathSettings")]
    public float GibsDamage = -50;
    public GameObject Gibs;
    public GameObject Corpse;
    public float CorpseLifeTime;
    public List<Drop> Drops;
[Header("LightAttackSettings")]
    public float LightAttackDamage;
    public float LightAttackRandomDamageOffsit;
    public float LightAttackRange;
    public float LightAttackCooldown;
    public float LightAttackKnockBack;
    public float LightAttackStunLength;
    public int LightAttackRayAmount;
[Header("Heavy Attack Settings")]
    [SerializeField]private float HeavyAttackCooldown;
    
[Header("Special Attack Settings")]
    [SerializeField]private float SpecialAttackCooldown;
    
[Header("Componenets")]
    public Rigidbody2D rig;
    [SerializeField]private EnemyBehavior behavior;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

[Header("Debug")]
    public bool DebugMode;
    public float LightAttackTimer = 0;
    public float HeavyAttackTimer = 0;
    public float SpecialAttackTimer = 0;
    

    private void Awake(){if (behavior)behavior.Properties = this;}

    private void Start(){GameServices.GlobalVariables.AllEnemies.Add(this);}

    public float GetHealth(){return Health;}
    public void SetHealth(float value){Health = value;}
    
    public void HitEnemy(float DamageDealt, Vector2 KnockBack, WeaponPropertiesHolder WeaponUsed){
        if (DamageDealt > 0){
            behavior.WeaponKilledBy = WeaponUsed;
            if (behavior) behavior.OnHit();
            Health -= DamageDealt;

            rig.AddForce(KnockBack, ForceMode2D.Impulse);

            AudioClip ClipToPlay = null;

            Vector2 dir = -rig.velocity.normalized;

            //get the angle of the direction (Radians)
            float rotate = Mathf.Atan2(dir.y,dir.x);

            GameObject Blood = Instantiate(BloodBurst, transform.position, Quaternion.Euler(0,0,rotate * Mathf.Rad2Deg - 90));

            //Rotate the Blood to face the KnockBack Direction
            ParticleSystem ps = Blood.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startRotation = rotate;

            if (WeaponUsed.WeaponType == WeaponPropertiesHolder.WeaponTypes.LongBlade || WeaponUsed.WeaponType == WeaponPropertiesHolder.WeaponTypes.ShortBlade){
                ClipToPlay = GameSounds.Instance.BladeImpactSound;
            }else if (WeaponUsed.WeaponType == WeaponPropertiesHolder.WeaponTypes.FireArm){
                ClipToPlay = GameSounds.Instance.BulletImpactSound;
            }else if (WeaponUsed.WeaponType == WeaponPropertiesHolder.WeaponTypes.Blunt){
                ClipToPlay = GameSounds.Instance.BluntImpactSound;
            }

           GameUtils.PlayAudio(ClipToPlay, transform.position, 1, 25);

            if (Health <= 0){
                GameServices.GlobalVariables.AllEnemies.Remove(this);
                behavior.DropItems();
                behavior.OnDeath();
                behavior.Die(KnockBack, Health);
            }
        }
    }

    [System.Serializable]
    public class Drop{
        [Range(0f, 1f)]
        public float Weight;
        public GameObject Object;
    }
    public enum State{
        Chasing,
        Roaming,
        Idle,
        Attacking,
        Spawning,
    }
    public enum Effects{
        Stunned,
    }
    public enum Direction{
        Up,
        Down,
        Left,
        Right
    }
}
