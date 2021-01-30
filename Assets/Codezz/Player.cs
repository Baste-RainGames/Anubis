using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Player : MonoBehaviour
{
    public float maxSpeed = 5;

    Rigidbody rb;
    Camera cam;
    public Transform playerModel;
    public Animator anim;
    #region Inputs
    Vector2 inputAxis;
    bool leftAttackDown;
    bool leftAttackUp;
    bool leftAttackHeld;
    bool rightAttackDown;
    bool rightAttackUp;
    bool rightAttackHeld;
    #endregion

    #region ItemStuff
    [FoldoutGroup("Items")]
    public Item heldItemLeft;
    [FoldoutGroup("Items")]
    public Item heldItemRight;
    [FoldoutGroup("Items")]
    public Item equipedItemHead;
    [FoldoutGroup("Items")]
    public Item equipedItemTorso;
    [FoldoutGroup("Items")]
    public Item equipedItemArmLeft;
    [FoldoutGroup("Items")]
    public Item equipedItemArmRight;
    [FoldoutGroup("Items")]
    public Item equipedItemLegLeft;
    [FoldoutGroup("Items")]
    public Item equipedItemLegRight;

    [FoldoutGroup("Item Slots")]
    public Transform leftHandSlot, rightHandSlot, headSlot, torsoSlot, leftArmSlot, rightArmSlot, leftLegSlot, rightLegSlot;

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
        AssignInputs();
        UpdateEquippedItems();
    }

    private void UpdateEquippedItems()
    {

    }

    private void AssignInputs()
    {
        inputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        leftAttackDown = Input.GetMouseButtonDown(0);
        leftAttackUp = Input.GetMouseButtonUp(0);
        leftAttackHeld = Input.GetMouseButton(0);
        rightAttackDown = Input.GetMouseButtonDown(1);
        rightAttackUp = Input.GetMouseButtonUp(1);
        rightAttackHeld = Input.GetMouseButton(1);
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