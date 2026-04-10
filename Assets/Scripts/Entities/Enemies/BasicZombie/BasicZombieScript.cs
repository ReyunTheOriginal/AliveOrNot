using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicZombieScript : EnemyBehavior
{
    public GameObject RightEyeCenter;
    public GameObject LeftEyeCenter;
    
    void Start()
    {
        
    }

    void Update()
    {
        GameUtils.MakeObjectLookAt(RightEyeCenter.transform, GameServices.GlobalVariables.Player.GameObject.transform.position);
        GameUtils.MakeObjectLookAt(LeftEyeCenter.transform, GameServices.GlobalVariables.Player.GameObject.transform.position);

        
        Properties.CurrentStats[EnemyProperties.Stat.Chasing] = true;

        if (Properties.CurrentStats[EnemyProperties.Stat.Chasing] && !Properties.CurrentStats[EnemyProperties.Stat.Stunned]){
            GameUtils.FollowObjectWithRig(Properties.rig, GameServices.GlobalVariables.Player.GameObject.transform, Properties.WalkSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.collider.gameObject.CompareTag("Player")){
            Vector2 dir = GameUtils.DirFromAToB(transform.position, other.transform.position);
            GameServices.GlobalVariables.Player.PlayerHealth.TakeDamage(Properties.Damage, Properties.KnockBack * dir, Properties.StunLength );
        }
    }
}
