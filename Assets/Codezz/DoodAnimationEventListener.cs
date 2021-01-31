using System;
using Animation_Player;
using UnityEngine;

public class DoodAnimationEventListener : MonoBehaviour {
    private void Start() {
        var ap = GetComponent<AnimationPlayer>();

        ap.RegisterAnimationEventListener("Attack1", OnAttack1);
        ap.RegisterAnimationEventListener("Attack2", OnAttack2);
    }

    public Dood dood;
    public void OnAttack1() {
        dood.ActivateHitbox();
    }

    public void OnAttack2() {
        dood.ActivateHitbox();
    }
}