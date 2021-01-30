using System;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {
    public Transform targetLimb;
    public BoxCollider box;
    public Vector3 offset;

    private Vector3 previousPosition;

    private void Awake() {
        transform.parent = null;
    }

    private void Update() {
        if (targetLimb == null) {
            Destroy(gameObject);
            return;
        }

        previousPosition = transform.position;
        transform.position = targetLimb.position + targetLimb.TransformVector(offset);
    }

    private static Collider[] colliderBuffer = new Collider[10];
    private static RaycastHit[] raycastHitBuffer = new RaycastHit[10];

    private List<Collider> results = new List<Collider>();
    public List<Collider> PollHit() {
        results.Clear();

        var overlapsAtCurrentPos = Physics.OverlapBoxNonAlloc(box.transform.position, box.size, colliderBuffer, transform.rotation);
        for (int i = 0; i < overlapsAtCurrentPos; i++)
            results.Add(colliderBuffer[i]);

        if (transform.position != previousPosition) {
            var deltaPosition = transform.position - previousPosition;
            Physics.BoxCastNonAlloc(transform.position, box.size, deltaPosition.normalized, raycastHitBuffer, transform.rotation, deltaPosition.magnitude, -1, QueryTriggerInteraction.Collide);
        }

        return results;
    }
}
