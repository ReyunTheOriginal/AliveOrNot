using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProperties : MonoBehaviour{
    public AudioSource audioSource;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [SerializeField]private float MaxHealth;
    [SerializeField]private float Health;
    public float Damage;
    public float RandomDamageOffsit;

    [SerializeField]private float LightAttackCooldown;
    [SerializeField]private float HeavyAttackCooldown;

    [SerializeField]private float SpecialAttackCooldown;

    public float KnockBack;
    public float StunLength;

    public float WalkSpeed;
    public GameObject Gibs;

    [SerializeField]private List<Drop> Drops;


    public Dictionary<State, bool> CurrentStates = new Dictionary<State, bool>{
        {State.Chasing, false},
        {State.Wandering, false},
        {State.Idle, false},
        {State.Attacking, false},
        {State.Spawning, false},
    };

    public Direction CurrentDirection;

    public Dictionary<Effects, float> CurrentEffects = new Dictionary<Effects, float>();
    [Header("Componenets")]
    public Rigidbody2D rig;
    [SerializeField]private EnemyBehavior behavior;

    private void Awake() {
        if (behavior)behavior.Properties = this;
    }

    private void Start(){
        behavior.AtSpawn();
        GameServices.GlobalVariables.AllEnemies.Add(this);
    }

    private void Update() {
        animator.SetInteger("WalkingDirection", (int)CurrentDirection);
        SetupStates();
        

        if (behavior){
            if (CurrentStates[State.Chasing])
                behavior.Chasing();
             if (CurrentStates[State.Wandering])
                behavior.Wandering();
             if (CurrentStates[State.Idle])
                behavior.Idling();
        }

        HashSet<Effects> EffectsToDelete = new HashSet<Effects>();

        // Get a temporary list of keys to iterate safely
        List<Effects> keys = new List<Effects>(CurrentEffects.Keys);

        foreach (var key in keys) {
            // Modify the value
            CurrentEffects[key] -= Time.deltaTime;

            // Check if it should be deleted
            if (CurrentEffects[key] <= 0) {
                EffectsToDelete.Add(key);
            }
        }

        foreach(Effects key in EffectsToDelete)
            CurrentEffects.Remove(key);

    }

    public void SetupStates(){
        animator.SetBool("Walking", (CurrentStates[State.Chasing] && !CurrentEffects.ContainsKey(Effects.Stunned)));

        Vector2 dir = GameUtils.DirFromAToB(transform.position, GameServices.GlobalVariables.Player.GameObject.transform.position);
        //Change Direction Based on where you're walking
        Vector2 TempInput = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

        if (TempInput.x > TempInput.y){
            if (dir.x > 0){
                CurrentDirection = Direction.Right;
            }else{
                CurrentDirection = Direction.Left;
            }
        }else{
            if (dir.y > 0){
                CurrentDirection = Direction.Up;
            }else{
                CurrentDirection = Direction.Down;
            }
        }

        switch (CurrentDirection){
            case Direction.Up:
                animator.SetInteger("WalkingDirection", 0);
                break;
            case Direction.Down:
                animator.SetInteger("WalkingDirection", 1);
                break;
            case Direction.Left:
                animator.SetInteger("WalkingDirection", 2);
                spriteRenderer.transform.rotation = Quaternion.Euler(0,180,0);
                break;
            default:
                animator.SetInteger("WalkingDirection", 3);
                spriteRenderer.transform.rotation = Quaternion.Euler(0,0,0);
                break;
        }

    }


    public void HitEnemy(float DamageDealt, Vector2 KnockBack, WeaponPropertiesHolder WeaponUsed){
        if (DamageDealt > 0){
            behavior.WeaponKilledBy = WeaponUsed;
            if (behavior) behavior.OnHit();
            ChangeHealth(-DamageDealt);

            rig.AddForce(KnockBack, ForceMode2D.Impulse);

            Debug.DrawRay(GameServices.GlobalVariables.Player.GameObject.transform.position, KnockBack, Color.green, 10f);

            AudioClip ClipToPlay = null;

            if (WeaponUsed.WeaponType == WeaponPropertiesHolder.WeaponTypes.LongBlade || WeaponUsed.WeaponType == WeaponPropertiesHolder.WeaponTypes.ShortBlade){
                ClipToPlay = GameSounds.Instance.BladeImpactSound;
            }else if (WeaponUsed.WeaponType == WeaponPropertiesHolder.WeaponTypes.FireArm){
                ClipToPlay = GameSounds.Instance.BulletImpactSound;
            }else if (WeaponUsed.WeaponType == WeaponPropertiesHolder.WeaponTypes.Blunt){
                ClipToPlay = GameSounds.Instance.BluntImpactSound;
            }

            audioSource.PlayOneShot(ClipToPlay);

            if (Health <= 0){
                GameServices.GlobalVariables.AllEnemies.Remove(this);
                DropItems();
                behavior.OnDeath();
                behavior.Die(KnockBack);
            }
        }
    }

    private void DropItems(){
        foreach (Drop chance in Drops){
            float num = UnityEngine.Random.Range(0f, 1f);
            if (chance.Matches(num)){
                Instantiate(chance.Object, transform.position, Quaternion.identity);
            }
        }
    }

    public void ChangeHealth(float Amount){
        Health += Amount;
    }

    [System.Serializable]
    public class Drop{
        [Range(0f, 1f)]
        public float Frequancy;
        public GameObject Object;

        public bool Matches(float num){
            return num <= Frequancy;
        }
    }
    public enum DeathTypes{
        gibs,
        Decapitation,
        DeathAnimation
    }
    public enum State{
        Chasing,
        Wandering,
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
