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
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        PositionDraggerUnderMouse(ray);

        if (Input.GetMouseButtonDown(0))
            TryDrag(ray);

        if (joint && Input.GetMouseButtonUp(0))
            LetGoOfDragged();
    }

    private void PositionDraggerUnderMouse(Ray ray) {
        var plane = new Plane(Vector3.back, dragger.transform.position);

        if (plane.Raycast(ray, out var distance)) {
            var hitPoint = ray.origin + distance * ray.direction;
            dragger.MovePosition(hitPoint);
        }
    }

    private void TryDrag(Ray ray) {
        if (Physics.Raycast(ray, out var hit)) {
            if (hit.rigidbody) {
                StartDragging(hit.rigidbody);
            }
        }
    }

    private void StartDragging(Rigidbody rb) {
        dragged = rb;
        dragged.isKinematic = false;
        joint = dragger.gameObject.AddComponent<SpringJoint>();

        var pos = dragger.transform.position;
        pos.y = rb.transform.position.y;
        dragger.position = pos;

        joint.connectedBody = rb;
        joint.autoConfigureConnectedAnchor = false;

        joint.connectedAnchor = Vector3.zero;
    }

    private void LetGoOfDragged() {
        Destroy(joint);

        PointerEventData dummyEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        dummyEventData.position = Input.mousePosition;

        var list = new List<RaycastResult>();
        rc.Raycast(dummyEventData, list);

        if (list.Count > 0) {
            var attachTo = list[0].gameObject.GetComponent<RectTransform>();

            var ray = mainCamera.ScreenPointToRay(uiCamera.WorldToScreenPoint(attachTo.position));
            var plane = new Plane(Vector3.back, dragger.transform.position);

            if (plane.Raycast(ray, out var distance)) {
                var worldPos = ray.origin + distance * ray.direction;
                dragged.velocity = Vector3.zero;
                dragged.isKinematic = true;
                dragged.MovePosition(worldPos);
            }

        }
    }
}