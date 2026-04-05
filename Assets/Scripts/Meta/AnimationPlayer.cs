using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationPlayer : MonoBehaviour
{
    PlayableGraph graph;
    AnimationPlayableOutput output;
    Animator animator;
    AnimationClipPlayable playable;

    public System.Func<IEnumerator> RunAfterAnimationIsOver;

    void Awake()
    {
        animator = GetComponent<Animator>();

        graph = PlayableGraph.Create();
        output = AnimationPlayableOutput.Create(graph, "Animation", animator);

        animator.enabled = false;
    }

    public void PlayClip(AnimationClip clip)
    {
        animator.enabled = true;

        animator.Rebind();
        animator.Update(0);

        graph.Stop();

        playable = AnimationClipPlayable.Create(graph, clip);
        output.SetSourcePlayable(playable);

        graph.Play();
    }

    void Update()
    {
        if (!graph.IsPlaying())
            return;

        if (playable.IsValid()){
            if (playable.GetTime() >= playable.GetAnimationClip().length){
                graph.Stop();
                animator.enabled = false;
                playable = default;

                if (RunAfterAnimationIsOver != null){
                    StartCoroutine(RunAfterAnimationIsOver());
                    RunAfterAnimationIsOver = null;
                }
            }
        }
    }

    void OnDestroy()
    {
        graph.Destroy();
    }
}

//how to use:

/*
other script:
    IEnumerator function(){
        yield return null;
    }

    AnimationPlayer.RunAfterAnimationIsOver = function;

*/