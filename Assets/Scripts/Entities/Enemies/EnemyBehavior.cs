using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour{
    [HideInInspector]public EnemyProperties Properties;
    [HideInInspector]public WeaponPropertiesHolder WeaponKilledBy;
    public float GibsLifeTime;

     private void Start() {
          AtSpawn();
     }

     private void Update() {

          Properties.CurrentStates[EnemyProperties.State.Chasing] = true;

          if (Properties.CurrentStates[EnemyProperties.State.Chasing] && !Properties.CurrentEffects.ContainsKey(EnemyProperties.Effects.Stunned) && !Properties.CurrentStates[EnemyProperties.State.Attacking]){
               GameUtils.FollowObjectWithRig(Properties.rig, GameServices.GlobalVariables.Player.GameObject.transform, Properties.WalkSpeed);
          }

          SetupStates();
          SetUpTimers();
          Properties.animator.SetInteger("WalkingDirection", (int)Properties.CurrentDirection);

          if (Properties.LightAttackTimer >= Properties.LightAttackCooldown && Vector2.Distance(GameServices.GlobalVariables.Player.GameObject.transform.position, transform.position) <= Properties.LightAttackRange){
               StartCoroutine(LightAttack());
               Properties.LightAttackTimer = 0;
          }
          

          if (Properties.CurrentStates[EnemyProperties.State.Chasing])
               Chasing();
          if (Properties.CurrentStates[EnemyProperties.State.Roaming])
               Roaming();
          if (Properties.CurrentStates[EnemyProperties.State.Idle])
               Idling();

          HashSet<EnemyProperties.Effects> EffectsToDelete = new HashSet<EnemyProperties.Effects>();

          // Get a temporary list of keys to iterate safely
          List<EnemyProperties.Effects> keys = new List<EnemyProperties.Effects>(Properties.CurrentEffects.Keys);

          foreach (var key in keys) {
               // Modify the value
               Properties.CurrentEffects[key] -= Time.deltaTime;

               // Check if it should be deleted
               if (Properties.CurrentEffects[key] <= 0) {
                    EffectsToDelete.Add(key);
               }
          }

          foreach(EnemyProperties.Effects key in EffectsToDelete)
               Properties.CurrentEffects.Remove(key);
    }

     public void SetUpTimers(){
          Properties.LightAttackTimer += Time.deltaTime;
          Properties.HeavyAttackTimer += Time.deltaTime;
          Properties.SpecialAttackTimer += Time.deltaTime;
     }

     public void SetupStates(){
          Properties.animator.SetBool("Walking", (Properties.CurrentStates[EnemyProperties.State.Chasing] || Properties.CurrentStates[EnemyProperties.State.Roaming]) && !Properties.CurrentEffects.ContainsKey(EnemyProperties.Effects.Stunned));

          Vector2 dir = GameUtils.DirFromAToB(transform.position, GameServices.GlobalVariables.Player.GameObject.transform.position);
          //Change Direction Based on where you're walking
          float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

          if (angle >= -45 && angle < 45)
               Properties.CurrentDirection = EnemyProperties.Direction.Right;
          else if (angle >= 45 && angle < 135)
               Properties.CurrentDirection = EnemyProperties.Direction.Up;
          else if (angle >= -135 && angle < -45)
               Properties.CurrentDirection = EnemyProperties.Direction.Down;
          else
               Properties.CurrentDirection = EnemyProperties.Direction.Left;

          if (!Properties.CurrentStates[EnemyProperties.State.Attacking]){
               switch (Properties.CurrentDirection){
                    case EnemyProperties.Direction.Up:
                         Properties.animator.SetInteger("WalkingDirection", 0);
                         break;
                    case EnemyProperties.Direction.Down:
                         Properties.animator.SetInteger("WalkingDirection", 1);
                         break;
                    case EnemyProperties.Direction.Left:
                         Properties.animator.SetInteger("WalkingDirection", 2);
                         Properties.spriteRenderer.transform.rotation = Quaternion.Euler(0,180,0);
                         break;
                    default:
                         Properties.animator.SetInteger("WalkingDirection", 3);
                         Properties.spriteRenderer.transform.rotation = Quaternion.Euler(0,0,0);
                         break;
               }
          }

    }

     public void DropItems(){
        foreach (EnemyProperties.Drop chance in Properties.Drops){
            float num = UnityEngine.Random.Range(0f, 1f);
            if (chance.Matches(num)){
                Instantiate(chance.Object, transform.position, Quaternion.identity);
            }
        }
    }


     //LightAttack, happens constantly, deals base Damage
     public virtual IEnumerator LightAttack(){
          Properties.animator.SetTrigger("Attack");
          Properties.CurrentStates[EnemyProperties.State.Attacking] = true;

          yield return new WaitForSeconds(0.3f);

          Vector2 dir = GameUtils.DirFromAToB(transform.position,GameServices.GlobalVariables.Player.GameObject.transform.position);
          
          HashSet<Collider2D> hits = GameUtils.SquareHitDetection(transform.position, dir, Properties.LightAttackRange * 0.9f, Properties.LightAttackRayAmount, 0.4f, "Player", GameUtils.LayerMaskFromNumbers(), Properties.DebugMode);

          if (hits.Count > 0)
               GameServices.GlobalVariables.Player.PlayerHealth.TakeDamage(Properties.LightAttackDamage, Properties.LightAttackKnockBack * dir, Properties.LightAttackStunLength );

          yield return new WaitForSeconds(0.1f);
          Properties.CurrentStates[EnemyProperties.State.Attacking] = false;
     }
     //Heavy Attack, happens more rarely and deals more Damage
     public virtual void HeavyAttack(){}
     //happens every once in a while, does something custom
     public virtual void SpecialAttack(){}
     //happens every Frame when the Enemy Is Chasing the Player
     public virtual void Chasing(){}
     //happens every Frame when the Enemy Is Wandering
     public virtual void Roaming(){}
     //happens every Frame when the Enemy Is Idle
     public virtual void Idling(){}
     //Happens once when the Enemy Spawns
     public virtual void AtSpawn(){}
     //Happens Once When the Enemy Dies
     public virtual void OnDeath(){
          Destroy(gameObject);
     }
     //Happens when the Enemy Gets Hit
     public virtual void OnHit(){}

     public virtual void SpawnGibs(Vector2 DeathKnockBack){
          SpawnGibsIE(DeathKnockBack).RunIndependent();
     }

     public virtual void StartDeathAnimation(){
          SpawnCorpseIE().RunIndependent();
     }

   public virtual void Die(Vector2 DeathKnockBack, float HealthAtDeath){
          AudioClip ClipToPlay = null;

          if (WeaponKilledBy.CanGib && HealthAtDeath <= Properties.GibsDamage){
               ClipToPlay = GameSounds.Instance.GibsExplosionSound;
               SpawnGibs(DeathKnockBack);
          }else {
               ClipToPlay = GameSounds.Instance.ZombieDeathSound;
               StartDeathAnimation();
          }

          GameUtils.PlayAudio(ClipToPlay, transform.position, 1, 25);
   }

     public IEnumerator SpawnCorpseIE(){
          GameObject corpse = Instantiate(Properties.Corpse, transform.position, Properties.spriteRenderer.transform.rotation);
          Animator animator = corpse.GetComponent<Animator>();
          if (animator)animator.Play("Death");
          yield return new WaitForSeconds(Properties.CorpseLifeTime);
          if (corpse)Destroy(corpse);
     }

     public IEnumerator SpawnGibsIE(Vector2 Velocity){
          Vector2 CachedPos = transform.position;
          Destroy(gameObject);
          GameObject GibsParent = Instantiate(Properties.Gibs, CachedPos, Quaternion.identity);

          foreach(Transform Gtran in GibsParent.transform){
               Rigidbody2D rig = Gtran.GetComponent<Rigidbody2D>();
               float stopTime = 0.9f;    // how long until basically stopped
               float threshold = 0.01f;  // "close enough to zero" (1% of original speed)

               float steps = stopTime / Time.fixedDeltaTime;
               float retainPerStep = Mathf.Pow(threshold, 1f / steps);
               rig.drag = (1f - retainPerStep) / Time.fixedDeltaTime;

               rig.velocity = GameUtils.AddRandomAngleToDir(Velocity.normalized, -40, 40) * Velocity.magnitude * UnityEngine.Random.Range(0.9f, 1.1f);
          }

          yield return new WaitForSeconds(GibsLifeTime * UnityEngine.Random.Range(0.9f, 1.1f));
          Destroy(GibsParent);
          yield return null;
    }
   
}
