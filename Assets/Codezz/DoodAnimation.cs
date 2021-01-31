using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoodAnimation : MonoBehaviour {
    public NavMeshAgent agent;
    public Animator animator;

    private string currentAnim;
    private float playActionUntil;

    private Dictionary<string, float> clipDurations = new Dictionary<string, float>();

    private void Start() {
        var clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips) {
            clipDurations[clip.name] = clip.length;
        }
    }

    public void Play(string animation) {
        if (clipDurations.TryGetValue(animation, out var duration)) {
            playActionUntil = Time.time + duration;
            animator.Play(animation);
        }
        else {
            Debug.Log($"can't find {animation} in [{string.Join(", ", clipDurations.Keys)}]");
        }
    }

    private void Update() {
        if (Time.time < playActionUntil)
            return;

        var targetAnim = agent.velocity.magnitude > .01f ? "zomb-walk" : "zomb-idle";

        if (targetAnim != currentAnim) {
            animator.Play(targetAnim);
            currentAnim = targetAnim;
        }
    }
}