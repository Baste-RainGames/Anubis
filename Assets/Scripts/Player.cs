using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float maxSpeed = 5;

    Rigidbody rb;
    Camera cam;
    Vector2 inputAxis;
    public Transform playerModel;
    public Animator anim;

    #region Pre Cinemachine Camera
    public Transform horizontalPivot, verticalPivot;

    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        cam = Camera.main;

    }

    public void Update()
    {
        inputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        MoveCameraTemp();
    }

    private void MoveCameraTemp()
    {
        horizontalPivot.Rotate(Vector3.up, Input.GetAxis("Mouse X"));
        verticalPivot.Rotate(Vector3.right, -Input.GetAxis("Mouse Y"));
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 forward = transform.position.RemoveY() - cam.transform.position.RemoveY();
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        var direction = (forward * inputAxis.y + right * inputAxis.x).normalized;
        if (direction.magnitude > 0.01f)
        {
            anim.SetFloat("MovementSpeed", direction.magnitude);
            playerModel.forward = direction;
        }
        else
        {
            anim.SetFloat("MovementSpeed", 0);
        }
        rb.velocity = direction * maxSpeed + Vector3.up * rb.velocity.y;
    }


}

public static class Vector3Extensions
{
    public static Vector3 RemoveY(this Vector3 v3)
    {
        v3.Scale(new Vector3(1, 0, 1));
        return v3;
    }
}