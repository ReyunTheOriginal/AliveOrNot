using System;
using UnityEngine;
public static class GameUtils{
    public static void MakeObjectLookAt(GameObject RotatedObject, Vector2 Target, float Offsit = 0, bool DontFlip = false){
        Vector2 dir = Target - (Vector2)RotatedObject.transform.position;

        if (dir.sqrMagnitude < 0.0001f) return;

        float rotate = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;

        if (DontFlip && (rotate >=90 || rotate <= -90)){
            RotatedObject.transform.rotation = Quaternion.Euler(0,180,-(rotate + Offsit + 180));
            return;
        }

        
        RotatedObject.transform.rotation = Quaternion.Euler(0,0,rotate + Offsit);
    }

    public static bool OverASpeedInDirection(Vector2 velocity, Vector2 dir, float speed){
        dir = dir.normalized;
        return Vector2.Dot(velocity, dir) > speed;
    }
}
