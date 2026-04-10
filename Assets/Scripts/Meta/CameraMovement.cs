using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("-Settings-")]
    public GameObject FollowTarget;
    public float Size = 5;
    public float SmoothFollowSpeed = 2;
    [Header("Toggles")]
    public bool Follows = true;
    public bool SmoothFollow = true;
    [Header("-Component-")]
    public Camera Camera;

    void LateUpdate()
    {
        if (Follows && FollowTarget != null){
            if (SmoothFollow){
                smoothFollow();
            }else{
                instantFollow();
            }
        }
        if (Size > 0.5f){
            Camera.orthographicSize = Size;
        }else{
            Camera.orthographicSize = 0.5f;
        }
    }
    
    private void instantFollow(){
        transform.position = new Vector3(FollowTarget.transform.position.x, FollowTarget.transform.position.y, -10);
    }
    private void smoothFollow(){
        Vector2 dir = (FollowTarget.transform.position - transform.position).normalized;
        float dis = Vector2.Distance(FollowTarget.transform.position,transform.position);

        transform.position += new Vector3((dir * SmoothFollowSpeed * dis).x, (dir * SmoothFollowSpeed * dis).y);
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
}
