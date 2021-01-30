using UnityEngine;

public class DoodAnimationEventListener : MonoBehaviour {
    public Dood dood;
    public void OnAttack() {
        dood.ActivateHitbox();
    }
}