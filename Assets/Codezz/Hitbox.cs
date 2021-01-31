using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {
    public BoxCollider box;

    public bool debugHit;
    public MeshRenderer debugRenderer;

    private static Collider[] colliderBuffer = new Collider[10];
    private static RaycastHit[] raycastHitBuffer = new RaycastHit[10];

    private List<Collider> results = new List<Collider>();
    private Coroutine debugRoutine;

    public List<Collider> PollHit() {
        results.Clear();

        var overlapsAtCurrentPos = Physics.OverlapBoxNonAlloc(box.transform.position, box.size, colliderBuffer, transform.rotation);
        for (int i = 0; i < overlapsAtCurrentPos; i++)
            results.Add(colliderBuffer[i]);

        if (debugHit) {
            if (debugRoutine != null)
                StopCoroutine(debugRoutine);
            debugRoutine = StartCoroutine(DebugRoutine());
        }

        return results;
    }

    private IEnumerator DebugRoutine() {
        debugRenderer.enabled = true;
        yield return new WaitForSeconds(.2f);
        debugRenderer.enabled = false;
    }
}
