using System.Collections.Generic;
using System.Linq;
using Animation_Player;
using UnityEngine;
using UnityEngine.AI;

public class DoodAnimation : MonoBehaviour {
    public NavMeshAgent agent;
    public AnimationPlayer anim;

    private string currentAnim;
    private float playActionUntil;

    private Dictionary<string, float> _clipDurations;
    public Dictionary<string, float> ClipDurations
        => _clipDurations ??= anim.layers[0].states.ToDictionary(state => state.Name, state => state.Duration);


    public float Play(string animation) {
        if (ClipDurations.TryGetValue(animation, out var duration)) {
            playActionUntil = Time.time + duration;
            anim.Play(anim.GetStateIndex(animation));
        }
        else {
            Debug.Log($"can't find {animation} in [{string.Join(", ", ClipDurations.Keys)}]");
        }

        return duration;
    }

    private void Update() {
        if (Time.time < playActionUntil || currentAnim == "zomb-die")
            return;

        var targetAnim = agent.velocity.magnitude > .01f ? "zomb-walk" : "zomb-idle";
        if (targetAnim != currentAnim) {
            Debug.Log("revert to " + targetAnim);
            anim.Play(anim.GetStateIndex(targetAnim));
            currentAnim = targetAnim;
        }
    }
}