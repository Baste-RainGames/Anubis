﻿using System.Collections.Generic;
using System.Linq;
using Animation_Player;
using UnityEngine;
using UnityEngine.AI;

public class DoodAnimation : MonoBehaviour {
    public NavMeshAgent agent;
    public AnimationPlayer anim;

    private float playActionUntil;

    private Dictionary<string, float> _clipDurations;
    public bool IsTurning;
    public bool HasAggro;

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
        if (Time.time < playActionUntil || anim.IsPlaying(anim.GetStateIndex("zomb-die")))
            return;

        var targetAnim = FindTargetAnim();
        if (targetAnim != anim.GetPlayingState().Name)
            anim.Play(anim.GetStateIndex(targetAnim));
    }

    private string FindTargetAnim() {
        if (IsTurning)
            return "zomb-turn";

        if (agent.velocity.magnitude > .01f)
            return HasAggro ? "zomb-chase" : "zomb-walk";
        else
            return "zomb-idle";
    }

    public bool IsPlaying(string animName) {
        return anim.IsPlaying(anim.GetStateIndex(animName));
    }
}