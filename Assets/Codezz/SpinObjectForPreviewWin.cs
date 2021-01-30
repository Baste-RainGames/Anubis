using UnityEngine;

public class SpinObjectForPreviewWin : MonoBehaviour {
    void Update() {
        transform.Rotate(Vector3.up, Time.deltaTime * 90f);
    }
}