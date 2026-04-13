using UnityEngine;

public class BasicZombieScript : EnemyBehavior
{
    public GameObject RightEyeCenter;
    public GameObject LeftEyeCenter;
    

    void Update(){
        
        Properties.CurrentStates[EnemyProperties.State.Chasing] = true;

        if (Properties.CurrentStates[EnemyProperties.State.Chasing] && !Properties.CurrentEffects.ContainsKey(EnemyProperties.Effects.Stunned)){
            EnemyAction.FollowPlayer(Properties.rig, Properties.WalkSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.collider.gameObject.CompareTag("Player")){
            Vector2 dir = GameUtils.DirFromAToB(transform.position, other.transform.position);
            GameServices.GlobalVariables.Player.PlayerHealth.TakeDamage(Properties.Damage, Properties.KnockBack * dir, Properties.StunLength );
        }
    }
}
