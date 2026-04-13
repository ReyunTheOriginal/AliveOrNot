using UnityEngine;

public static class EnemyAction {

    public static void FollowPlayer(Rigidbody2D rig, float Speed, bool PathFinding = true){
        GameUtils.FollowObjectWithRig(rig, GameServices.GlobalVariables.Player.GameObject.transform, Speed);
    }
    
}
