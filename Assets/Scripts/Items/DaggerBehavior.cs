using UnityEngine;
using Unity.Mathematics;

public class DaggerBehavior : ItemBehavior
{

    public override void Hold(){

        Vector2 MouseRawPos = Input.mousePosition;
        Vector2 MousePos = Camera.main.ScreenToWorldPoint(MouseRawPos);
        Vector2 dir = (MousePos - (Vector2)GameServices.GlobalVariables.Player.transform.position).normalized;

        float rotate = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;

        GameServices.GlobalVariables.OffHandObject.Center.transform.rotation = Quaternion.Euler(0,0,rotate);
    }
}