using System.Collections.Generic;
using Animation_Player;
using UnityEngine;
using UnityEngine.AI;

public class DoodAnimation : MonoBehaviour {
    public NavMeshAgent agent;
    public AnimationPlayer anim;

    private string currentAnim;
    private float playActionUntil;

    private Dictionary<string, float> clipDurations = new Dictionary<string, float>();

    private void Start() {
        foreach (var state in anim.layers[0].states) {
            clipDurations[state.Name] = state.Duration;
        }
    }

    public float Play(string animation) {
        if (clipDurations.TryGetValue(animation, out var duration)) {
            playActionUntil = Time.time + duration;
            anim.Play(anim.GetStateIndex(animation));
        }
        else {
            Debug.Log($"can't find {animation} in [{string.Join(", ", clipDurations.Keys)}]");
        }

        return duration;
    }

    private void Update() {
        if (Time.time < playActionUntil || currentAnim == "zomb-die")
            return;

        var targetAnim = agent.velocity.magnitude > .01f ? "zomb-walk" : "zomb-idle";

        if (targetAnim != currentAnim) {
            anim.Play(anim.GetStateIndex(targetAnim));
            currentAnim = targetAnim;
        }
    }
}