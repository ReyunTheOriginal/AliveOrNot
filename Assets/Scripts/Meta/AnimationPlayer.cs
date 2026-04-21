using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationPlayer : MonoBehaviour
{
    PlayableGraph graph;
    AnimationPlayableOutput output;
    AnimationClipPlayable playable;

    public System.Func<IEnumerator> RunAfterAnimationIsOver;

    Coroutine currentAnimation;

    bool isInitialized = false;


    void Awake(){
        var animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = null; // 🔥 kill the controller here
        graph = PlayableGraph.Create();
        output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        graph.Play();
        isInitialized = true;
    }

    void OnEnable(){
        if (!isInitialized) return; // 🔥 skip if Awake hasn't run yet

        if (output.IsOutputValid())
            output.SetSourcePlayable(Playable.Null);

        if (playable.IsValid())
            playable.Destroy();

        playable = default;

        if (graph.IsValid())
            graph.Play();
    }

    void OnDisable(){
        if (!isInitialized) return;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        if (output.IsOutputValid())
            output.SetSourcePlayable(Playable.Null);

        if (playable.IsValid())
            playable.Destroy();

        playable = default;

        if (graph.IsValid())
            graph.Stop();

        // 🔥 Removed animator.Rebind/Update — can't call on inactive object
        // The null controller means there's nothing to reset anyway
    }

    public void PlayClip(AnimationClip clip){
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        if (playable.IsValid())
            playable.Destroy();

        output.SetSourcePlayable(Playable.Null); // 🔥 disconnect before reassigning

        playable = AnimationClipPlayable.Create(graph, clip);
        output.SetSourcePlayable(playable);
        playable.SetTime(0);
        graph.Evaluate(); // 🔥 force first frame immediately so there's no 1-frame delay

        currentAnimation = StartCoroutine(WaitForAnimation(clip));
    }

    public void StopAnimation(){
        // Stop the running coroutine
        if (currentAnimation != null){
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        // Disconnect animation from animator
        if (output.IsOutputValid()){
            output.SetSourcePlayable(Playable.Null);
        }

        // Destroy playable safely
        if (playable.IsValid()){
            playable.Destroy();
        }

        playable = default;

        // Optional callback
        if (RunAfterAnimationIsOver != null){
            StartCoroutine(RunAfterAnimationIsOver());
            RunAfterAnimationIsOver = null;
        }
    }


    IEnumerator WaitForAnimation(AnimationClip clip){
        float elapsed = 0f;
        while (elapsed < clip.length)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Force final frame
        if (playable.IsValid())
        {
            playable.SetTime(clip.length);
            graph.Evaluate();
        }

        yield return new WaitForEndOfFrame();

        output.SetSourcePlayable(Playable.Null);
        if (playable.IsValid())
            playable.Destroy();
        playable = default;
        currentAnimation = null;

        if (RunAfterAnimationIsOver != null)
        {
            yield return StartCoroutine(RunAfterAnimationIsOver());
            RunAfterAnimationIsOver = null;
        }
    }

    void OnDestroy(){
        graph.Destroy();
    }
}