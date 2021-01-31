using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

public class Player : MonoBehaviour
{
    public float maxSpeed = 5;

    Rigidbody rb;

    Vector3 facingDirection = Vector3.forward;

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

    List<ItemSlot> _itemSlots = new List<ItemSlot>();
    List<ItemSlot> ItemSlots
    {
        get
        {
            if (_itemSlots == null || _itemSlots.Count == 0)
            {
                _itemSlots.Add(leftHandSlot);
                _itemSlots.Add(rightHandSlot);
                _itemSlots.Add(headSlot);
                _itemSlots.Add(torsoSlot);
                _itemSlots.Add(leftArmSlot);
                _itemSlots.Add(rightArmSlot);
                _itemSlots.Add(leftLegSlot);
                _itemSlots.Add(rightLegSlot);

            }
            return _itemSlots;
        }
    }


    #endregion

    public int MaxHealth => 10;
    public int currentHealth;
    public float throwStrength = 7;
    
    private bool freezePlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        UpdateEquippedItems();  
        cam = Camera.main;

        currentHealth = MaxHealth;
        if (!FindObjectOfType<HPUI>())
            Instantiate(Resources.Load("HP Canvas"));
        foreach (var kvp in TransferDataBetweenScenes.equipedToSlotWhenExitingLostNFound) {
            var (id, item) = kvp;

            var slot = id switch {
                ItemSlotID.leftHandSlot  => leftHandSlot,
                ItemSlotID.rightHandSlot => rightHandSlot,
                ItemSlotID.headSlot      => headSlot,
                ItemSlotID.torsoSlot     => torsoSlot,
                ItemSlotID.leftArmSlot   => leftArmSlot,
                ItemSlotID.rightArmSlot  => rightArmSlot,
                ItemSlotID.leftLegSlot   => leftLegSlot,
                ItemSlotID.rightLegSlot  => rightLegSlot,
                _ => throw new ArgumentOutOfRangeException(nameof(id), id.ToString())
            };

            slot.item = item;
        }

        UpdateEquippedItems();
    }

    public void Update()
    {
        AssignInputs();         
        if (freezePlayer)
            return;

        if (rightAttackDown)
            switch (rightHandSlot.item.itemType)
            {
                case ItemType.Thrown:
                    anim.Play("throw");
                    freezePlayer = true;
                    rb.velocity = Vector3.zero;
                    Invoke("ThrowRight", .4f);
                    break;
                case ItemType.Projectile:
                    break;
                case ItemType.Club:
                    anim.Play("player-club-R");
                    break;
                case ItemType.Sharp:
                    anim.Play("weapon_swing");
                    break;
                case ItemType.Squeeker:
                    break;
                default:
                    throw new Exception("Item type has no case yet");
            }
        if (leftAttackDown)
            switch (leftHandSlot.item.itemType)
            {
                case ItemType.Thrown:
                    anim.Play("player-throw-L");
                    freezePlayer = true;
                    rb.velocity = Vector3.zero;
                    Invoke("ThrowLeft", .4f);
                    break;
                case ItemType.Projectile:
                    break;
                case ItemType.Club:
                    anim.Play("player-club-L");
                    break;
                case ItemType.Sharp:
                    anim.Play("player-slash-L");
                    break;
                case ItemType.Squeeker:
                    break;
                default:
                    throw new Exception("Item type has no case yet");
            }

    }

    void ThrowRight()
    {
        Throw(rightHandSlot.transform.position, rightHandSlot.item.itemPrefab);
    }
    void ThrowLeft()
    {
        Throw(leftHandSlot.transform.position, leftHandSlot.item.itemPrefab);
    }

    void Throw(Vector3 positionToThrowFrom, GameObject prefab)
    {
        var o = Instantiate(prefab);
        o.transform.position = positionToThrowFrom;
        o.AddComponent<Rigidbody>().velocity = (facingDirection + transform.up * .3f).normalized * throwStrength;

        freezePlayer = false;

    }

    [ContextMenu("Update Equipment")]
    private void UpdateEquippedItems()
    {
        for (int i = 0; i < ItemSlots.Count; i++)
        {
            var slot = ItemSlots[i];
            if (slot.transform.childCount > 0)
                Destroy(slot.transform.GetChild(0).gameObject);
            if(slot.item != null)
            {
                var equipedItem = Instantiate(slot.item.itemPrefab);
                equipedItem.transform.parent = slot.transform;
                equipedItem.layer = slot.gameObject.layer;
                equipedItem.transform.localPosition = Vector3.zero + slot.item.offsetPosition;
                equipedItem.transform.rotation = slot.transform.rotation * Quaternion.Euler(slot.item.offsetRotation);
            }
        }
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
        if (freezePlayer)
            return;
        Move();
    }

    private void Move()
    {
        Vector3 forward = transform.position.RemoveY() - cam.transform.position.RemoveY();
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        var direction = (forward * inputAxis.y + right * inputAxis.x).normalized;
        if (direction.magnitude > 0.01f)
        {
            anim.SetFloat("MovementSpeed", facingDirection.magnitude);
            playerModel.forward = facingDirection = direction;
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

