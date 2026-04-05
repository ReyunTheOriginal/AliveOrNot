using UnityEngine;

public class EnemyBehavior : MonoBehaviour{
    [HideInInspector]public EnemyProperties Properties;

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
   
}
