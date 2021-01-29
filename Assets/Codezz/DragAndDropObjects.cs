using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDropObjects : MonoBehaviour {

    public Camera mainCamera;
    public Camera uiCamera;
    public Rigidbody dragger;
    public GraphicRaycaster rc;

    private SpringJoint joint;
    private Rigidbody dragged;

    void Update() {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.back, dragger.transform.position);

        if (plane.Raycast(ray, out var distance)) {
            var hitPoint = ray.origin + distance * ray.direction;
            dragger.MovePosition(hitPoint);
        }

        if (Input.GetMouseButtonDown(0))
            TryDrag(ray);

        if (joint && Input.GetMouseButtonUp(0))
            HandleDraggedLetGoOf();
    }

    private void TryDrag(Ray ray) {
        if (Physics.Raycast(ray, out var hit)) {
            if (hit.rigidbody) {
                Drag(hit.rigidbody);
            }
        }
    }

    private void HandleDraggedLetGoOf() {
        Destroy(joint);

        PointerEventData dummyEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        dummyEventData.position = Input.mousePosition;

        var list = new List<RaycastResult>();
        rc.Raycast(dummyEventData, list);

        if (list.Count > 0) {
            var attachTo = list[0].gameObject.GetComponent<RectTransform>();

            var screenPos = uiCamera.WorldToScreenPoint(attachTo.position);
            var worldPos = mainCamera.ScreenToWorldPoint(screenPos);

            worldPos.z = dragged.transform.position.z;
            dragged.velocity = Vector3.zero;
            dragged.isKinematic = true;
            dragged.MovePosition(worldPos);
        }
    }

    private void Drag(Rigidbody rb) {
        dragged = rb;
        joint = dragger.gameObject.AddComponent<SpringJoint>();

        var pos = dragger.transform.position;
        pos.y = rb.transform.position.y;
        dragger.position = pos;

        joint.connectedBody = rb;
        joint.autoConfigureConnectedAnchor = false;

        joint.connectedAnchor = Vector3.zero;
    }
}