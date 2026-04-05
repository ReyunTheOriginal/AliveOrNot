using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationPlayer : MonoBehaviour
{
    PlayableGraph graph;
    AnimationPlayableOutput output;

    void Awake()
    {
        Animator animator = GetComponent<Animator>();

        graph = PlayableGraph.Create();
        output = AnimationPlayableOutput.Create(graph, "Animation", animator);
    }

    public void PlayClip(AnimationClip clip)
    {
        graph.Destroy();

        graph = PlayableGraph.Create();

        var playable = AnimationClipPlayable.Create(graph, clip);

        output = AnimationPlayableOutput.Create(graph, "Animation", GetComponent<Animator>());
        output.SetSourcePlayable(playable);

        graph.Play();
    }
}