using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraScript : MonoBehaviour
{
    [Header("-Settings-")]
    public GameObject FollowTarget;
    public float Size = 5;
    public float SmoothFollowSpeedLerp = 0.1f;
    public PixelPerfectCamera pixelCam;
    [SerializeField] int minPixelSize = 90;   // more pixels = zoomed out
    [SerializeField] int maxPixelSize = 270;  // fewer pixels = zoomed in
    [SerializeField] int zoomStep = 30;  // must be divisible into your base res
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

        if (!UIManager.AMenuIsOpened()){
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll < 0f)
                pixelCam.assetsPPU = Mathf.Max(minPixelSize, pixelCam.assetsPPU - zoomStep);
            else if (scroll > 0f)
                pixelCam.assetsPPU = Mathf.Min(maxPixelSize, pixelCam.assetsPPU + zoomStep);
        }
    }
    
    private void instantFollow(){
        transform.position = new Vector3(FollowTarget.transform.position.x, FollowTarget.transform.position.y, -10);
    }
    private void smoothFollow(){
        Vector2 TargetPos = (Vector2)FollowTarget.transform.position + GameServices.GlobalVariables.Player.MovementScript.DirectionalSpeed;
        float dis = Vector2.Distance(TargetPos,transform.position);

        Vector3 newPos = new Vector3(
            Mathf.Lerp(transform.position.x, TargetPos.x, SmoothFollowSpeedLerp * dis * Time.deltaTime), 
            Mathf.Lerp(transform.position.y, TargetPos.y, SmoothFollowSpeedLerp * dis * Time.deltaTime),
            -10
        );

        transform.position = newPos;
    }
}
