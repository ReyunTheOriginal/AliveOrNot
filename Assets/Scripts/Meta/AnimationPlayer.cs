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

        // 🔥 Destroy the old node before creating a new one
        if (playable.IsValid())
            playable.Destroy();

        playable = AnimationClipPlayable.Create(graph, clip);
        output.SetSourcePlayable(playable);
        playable.SetTime(0);

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


    IEnumerator WaitForAnimation(AnimationClip clip) {
        // 1. Wait until the playable has actually finished based on its own clock
        while (playable.IsValid() && !playable.IsDone() && playable.GetTime() < clip.length) {
            yield return null; 
        }

        // 2. IMPORTANT: Force the graph to evaluate the exact final frame
        if (playable.IsValid()) {
            playable.SetTime(clip.length);
            graph.Evaluate(); 
        }

        // 3. Give the engine one frame to render this final pose
        yield return new WaitForEndOfFrame();

        // Now it is safe to cleanup
        output.SetSourcePlayable(Playable.Null);
        
        if (playable.IsValid())
            playable.Destroy();

        playable = default;
        currentAnimation = null;

        if (RunAfterAnimationIsOver != null) {
            yield return StartCoroutine(RunAfterAnimationIsOver());
            RunAfterAnimationIsOver = null;
        }
    }

    void OnDestroy(){
        graph.Destroy();
    }
}