using System.Collections;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour{
    [HideInInspector]public EnemyProperties Properties;
    [HideInInspector]public WeaponPropertiesHolder WeaponKilledBy;
    public float GibsLifeTime;

   //basic Attack, happens often
   public virtual void LightAttack(){

   }
   //Heavy Attack, happens more rarely and deals more Damage
   public virtual void HeavyAttack(){
    
   }
   //happens every once in a while, does something custom
   public virtual void SpecialAttack(){

   }
   //happens every Frame when the Enemy Is Chasing the Player
   public virtual void Chasing(){

   }
   //happens every Frame when the Enemy Is Wandering
   public virtual void Wandering(){

   }
   //happens every Frame when the Enemy Is Idle
   public virtual void Idling(){

   }
   //Happens once when the Enemy Spawns
   public virtual void AtSpawn(){

   }
   //Happens Once When the Enemy Dies
   public virtual void OnDeath(){
        Destroy(gameObject);
   }
   //Happens when the Enemy Gets Hit
   public virtual void OnHit(){
    
   }

   public virtual void SpawnGibs(Vector2 DeathKnockBack){
     SpawnGibsIE(DeathKnockBack).RunIndependent();
   }

   public virtual void StartDeathAnimation(){
    
   }

   public virtual void Decapitate(){
    
   }

   public virtual void Die(Vector2 DeathKnockBack){
          if (WeaponKilledBy.KillType == EnemyProperties.DeathTypes.gibs){
               SpawnGibs(DeathKnockBack);
          }else if (WeaponKilledBy.KillType == EnemyProperties.DeathTypes.Decapitation){
               Decapitate();
          }else if (WeaponKilledBy.KillType == EnemyProperties.DeathTypes.DeathAnimation){
               StartDeathAnimation();
          }

          AudioClip ClipToPlay = null;

          if (WeaponKilledBy.KillType == EnemyProperties.DeathTypes.gibs){
               ClipToPlay = GameSounds.Instance.GibsExplosionSound;
          }else if (WeaponKilledBy.KillType == EnemyProperties.DeathTypes.Decapitation){
               ClipToPlay = GameSounds.Instance.DecapitationSound;
          }

          GameUtils.PlayIndependentAudio(ClipToPlay, transform.position);
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
