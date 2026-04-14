using System;
using System.Collections;
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

    public void StartTempAudio(AudioClip clip, float VolumeFallOff, float MaxDistanceToHear){
        AudioSource source = gameObject.AddComponent<AudioSource>();
        StartCoroutine(StartTemoAudioCor(source, clip, VolumeFallOff, MaxDistanceToHear));
    }
    IEnumerator StartTemoAudioCor(AudioSource source, AudioClip clip, float VolumeFallOff, float MaxDistanceToHear){
        float distance = Vector2.Distance(transform.position, GameServices.GlobalVariables.Camera.transform.position);
    
        // 0 at maxDistance, 1 at center
        float t = Mathf.Clamp01(1f - (distance / MaxDistanceToHear));
        
        // apply falloff curve — higher volumeFallOff = quiets faster with distance
        float volume = Mathf.Pow(t, VolumeFallOff);

        source.clip = clip;
        source.volume = volume;
        source.Play();

        yield return new WaitUntil(() => !source.isPlaying);
        Destroy(gameObject);
    }
}
