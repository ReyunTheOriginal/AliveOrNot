using UnityEngine;

public class GibScript : MonoBehaviour
{
    public Rigidbody2D rig;
    private void OnValidate() {
        rig = GetComponent<Rigidbody2D>();
    }
    private void Update() {
       /* rig.velocity = GameUtils.ClampVector2(rig.velocity, 3f, 39f);
        if (rig.velocity != Vector2.zero){
            float angle = Mathf.Atan2(rig.velocity.y, rig.velocity.x) * Mathf.Rad2Deg + UnityEngine.Random.Range(-35, 35);
            rig.rotation = angle;
        }*/
    }
}
