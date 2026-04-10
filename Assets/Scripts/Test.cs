using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public bool BS = false;
    public float FirstDelay;
    public GameObject Enemy;
    public float BaseDelay;
    public float FinalDelay;
    public float Range;
    public float time;
    public float SpeedIncreaseSpeed;

    private void Start() {
        StartCoroutine(Spawn());
    }

    private void Update() {
        time += Time.deltaTime;
        FinalDelay = (BaseDelay / (time * 0.1f * SpeedIncreaseSpeed)) - (FirstDelay * 0.1f * SpeedIncreaseSpeed);
    }

    IEnumerator Spawn(){
        yield return new WaitForSeconds(FirstDelay);
        while (true){
            Instantiate(Enemy, GameUtils.GetRandomPointInSquareRange(GameServices.GlobalVariables.Player.GameObject.transform.position, Range), Quaternion.identity);
            if (!BS)yield return new WaitForSeconds(FinalDelay + 0.01f);
            if (BS) yield return null;
        }   
    }
}
