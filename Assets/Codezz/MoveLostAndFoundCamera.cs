using Cinemachine;
using UnityEngine;

public class MoveLostAndFoundCamera : MonoBehaviour {

    public float speed;
    public Camera cam;

    public CinemachineVirtualCamera vCam;

    void Update() {
        var mousePos = cam.ScreenToViewportPoint(Input.mousePosition);

        if (mousePos.x < .1f && vCam.State.PositionCorrection.x <= 0.0001) {
            transform.Translate(new Vector3(-Time.deltaTime * speed, 0f, 0f));
        }
        else if (mousePos.x > .9f  && vCam.State.PositionCorrection.x >= -0.0001) {
            transform.Translate(new Vector3(Time.deltaTime * speed, 0f, 0f));
        }

        if (mousePos.y < .1f && vCam.State.PositionCorrection.y <= 0.0001) {
            transform.Translate(new Vector3(0f, -Time.deltaTime * speed, 0f));
        }
        else if (mousePos.y > .9f  && vCam.State.PositionCorrection.y >= -0.0001) {
            transform.Translate(new Vector3(0f, Time.deltaTime * speed, 0f));
        }
    }
}