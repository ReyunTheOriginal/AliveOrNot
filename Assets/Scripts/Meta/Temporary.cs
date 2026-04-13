using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Temporary : MonoBehaviour
{
    public GameUtils.IndependentCoroutine StartTempCoroutine(System.Func<IEnumerator> func){
        var ICoroutine = new GameUtils.IndependentCoroutine();
        ICoroutine.Owner = this;

        ICoroutine.Coroutine = StartCoroutine(StartTemoCoroutineCor(func, ICoroutine));

        return ICoroutine;
    }

    IEnumerator StartTemoCoroutineCor(System.Func<IEnumerator> func, GameUtils.IndependentCoroutine ICor){
        yield return StartCoroutine(func());

        // mark as finished
        ICor.Coroutine = null;

        // cleanup
        Destroy(gameObject);
    }

    public void StartTempAudio(AudioClip clip){
        AudioSource source = gameObject.AddComponent<AudioSource>();
        StartCoroutine(StartTemoAudioCor(source, clip));
    }
    IEnumerator StartTemoAudioCor(AudioSource source, AudioClip clip){
        source.PlayOneShot(clip);
        yield return new WaitUntil(() => !source.isPlaying);
        // cleanup
        Destroy(gameObject);
    }
}
