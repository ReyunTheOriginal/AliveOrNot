using System.Collections;
using UnityEngine;

public class Temporary : MonoBehaviour
{
    public void StartTempCoroutine(System.Func<IEnumerator> func){
        StartCoroutine(TempCoroutine(func));
    }

    IEnumerator TempCoroutine(System.Func<IEnumerator> func){
        yield return StartCoroutine(func());
        Destroy(gameObject);
    }
}
