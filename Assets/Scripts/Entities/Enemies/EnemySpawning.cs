using System.Collections;
using UnityEngine;

public class EnemySpawning : MonoBehaviour
{
    public float FirstDelay;
    public GameObject Enemy;
    public float BaseDelay;
    public float Range;

    private void Start() {
        StartCoroutine(Spawn());
    }

    private void Update() {
    }

    IEnumerator Spawn(){
        yield return new WaitForSeconds(FirstDelay);
        while (true){
            if (!GameServices.IsDayTime){
                yield return new WaitForSeconds(BaseDelay + 0.01f);
                if (!GameServices.IsDayTime) Instantiate(Enemy, GameUtils.GetRandomPointInCircleRange(GameServices.GlobalVariables.Player.GameObject.transform.position, Range, Range - 10), Quaternion.identity);
            }  
            yield return null;
        }
    }
}
