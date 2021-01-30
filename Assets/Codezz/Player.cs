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
    [FoldoutGroup("Item Slots")]
    [InlineEditor]
    public ItemSlot leftHandSlot, rightHandSlot, headSlot, torsoSlot, leftArmSlot, rightArmSlot, leftLegSlot, rightLegSlot;

    List<ItemSlot> _itemSlots;
    List<ItemSlot> ItemSlots => _itemSlots ??= new List<ItemSlot>() { leftArmSlot, rightArmSlot, headSlot, torsoSlot, leftArmSlot, rightArmSlot, leftLegSlot, rightLegSlot};

    [FoldoutGroup("Item Slots")]
    [Button]
    void ToggleAllGizmos(bool isShown = true)
    {
        leftHandSlot.showSlotGizmos = isShown;
        rightHandSlot.showSlotGizmos = isShown;
        headSlot.showSlotGizmos = isShown;
        torsoSlot.showSlotGizmos = isShown;
        leftArmSlot.showSlotGizmos = isShown;
        rightArmSlot.showSlotGizmos = isShown;
        leftLegSlot.showSlotGizmos = isShown;
        rightLegSlot.showSlotGizmos = isShown;
    }
    #endregion


    public int MaxHealth => 10;
    public int currentHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        cam = Camera.main;

        currentHealth = MaxHealth;
        if (!FindObjectOfType<HPUI>())
            Instantiate(Resources.Load("HP Canvas"));
    }

    public void Update()
    {
        AssignInputs();         
        UpdateEquippedItems();  

        if (rightAttackDown)
            switch (rightHandSlot.item.itemType)
            {
                case ItemType.Thrown:
                    anim.Play("throw");
                    break;
                case ItemType.Projectile:
                    break;
                case ItemType.Club:
                case ItemType.Sharp:
                    anim.Play("weapon_swing");
                    break;
                case ItemType.Squeeker:
                    break;
                default:
                    throw new Exception("Item type has no case yet");
            }
        
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

    public void OnHit() {
        currentHealth = Mathf.Max(0, currentHealth - 1);
    }
}

