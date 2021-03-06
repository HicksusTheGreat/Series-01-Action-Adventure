﻿using UnityEngine;
using System.Collections;
using System.Runtime.Remoting.Messaging;

public class CharacterMovementModel : MonoBehaviour 
{
    public float Speed;
    public Transform WeaponParent;
    public Transform ShieldParent;
    public Transform PickupItemParent;

    private Vector3 m_MovementDirection;
    private Vector3 m_FacingDirection;

    private Rigidbody2D m_Body;

    private bool m_IsFrozen;
    private bool m_IsAttacking;
    private ItemType m_PickingUpObject = ItemType.None;

    private ItemType m_EquippedWeapon = ItemType.None;
    private ItemType m_EquippedShield = ItemType.None;

    private GameObject m_PickupItem;

    private Vector2 m_PushDirection;
    private float m_PushTime;

    private int m_LastSetDirectionFrameCount;

    void Awake()
    {
        m_Body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdatePushTime();
    }

    void FixedUpdate()
    {
        UpdateMovement();
    }

    void LateUpdate()
    {
        

    }

    void UpdatePushTime()
    {
        m_PushTime = Mathf.MoveTowards( m_PushTime, 0f, Time.deltaTime );
    }

    void UpdateMovement()
    {
        if( m_IsFrozen == true || m_IsAttacking == true )
        {
            m_Body.velocity = Vector2.zero;
            return;
        }

        if( m_MovementDirection != Vector3.zero )
        {
            m_MovementDirection.Normalize();
        }

        if( IsBeingPushed() == true )
        {
            m_Body.velocity = m_PushDirection;
        }
        else
        {
            m_Body.velocity = m_MovementDirection * Speed;
        }
    }

    public bool IsBeingPushed()
    {
        return m_PushTime > 0;
    }

    public bool IsFrozen()
    {
        return m_IsFrozen;
    }

    public void SetFrozen( bool isFrozen, bool affectGameTime )
    {
        m_IsFrozen = isFrozen;

        if( affectGameTime == true )
        {
            if( isFrozen == true )
            {
                StartCoroutine( FreezeTimeRoutine() );
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }

    IEnumerator FreezeTimeRoutine()
    {
        yield return null;

        Time.timeScale = 0;
    }

    public void SetDirection( Vector2 direction )
    {
        if( direction != Vector2.zero &&
            GetItemThatIsBeingPickedUp() != ItemType.None )
        {
            m_PickingUpObject = ItemType.None;
            SetFrozen( false, true );
            Destroy( m_PickupItem );
        }

        if( m_IsFrozen == true || m_IsAttacking == true )
        {
            return;
        }

        if( IsBeingPushed() == true )
        {
            m_MovementDirection = m_PushDirection;
            return;
        }

        if( Time.frameCount == m_LastSetDirectionFrameCount )
        {
            return;
        }

        m_MovementDirection = new Vector3( direction.x, direction.y, 0 );

        if( direction != Vector2.zero )
        {
            m_FacingDirection = m_MovementDirection;
            m_LastSetDirectionFrameCount = Time.frameCount;
        }
    }

    public Vector3 GetDirection()
    {
        return m_MovementDirection;
    }

    public Vector3 GetFacingDirection()
    {
        return m_FacingDirection;
    }

    public bool IsMoving()
    {
        if( m_IsFrozen == true )
        {
            return false;
        }

        return m_MovementDirection != Vector3.zero;
    }

    public void EquipWeapon( ItemType itemType )
    {
        EquipItem( itemType, ItemData.EquipPosition.SwordHand, WeaponParent, ref m_EquippedWeapon );
    }

    public void EquipShield( ItemType itemType )
    {
        EquipItem( itemType, ItemData.EquipPosition.ShieldHand, ShieldParent, ref m_EquippedShield );
    }

    void EquipItem( ItemType itemType, ItemData.EquipPosition equipPosition, 
                    Transform itemParent, ref ItemType equippedItemSlot )
    {
        if( itemParent == null )
        {
            return;
        }

        ItemData itemData = Database.Item.FindItem( itemType );

        if( itemData == null )
        {
            return;
        }

        if( itemData.IsEquipable != equipPosition )
        {
            return;
        }

        equippedItemSlot = itemType;

        GameObject newItemObject = (GameObject)Instantiate( itemData.Prefab );

        newItemObject.transform.parent = itemParent;
        newItemObject.transform.localPosition = Vector2.zero;
        newItemObject.transform.localRotation = Quaternion.identity;
    }

    public void ShowItemPickup( ItemType itemType )
    {
        if( PickupItemParent == null )
        {
            return;
        }

        ItemData itemData = Database.Item.FindItem( itemType );

        if( itemData == null )
        {
            return;
        }

        SetDirection( new Vector2( 0, -1 ) );
        SetFrozen( true, true );

        m_PickingUpObject = itemType;

        m_PickupItem = (GameObject)Instantiate( itemData.Prefab );

        m_PickupItem.transform.parent = PickupItemParent;
        m_PickupItem.transform.localPosition = Vector2.zero;
        m_PickupItem.transform.localRotation = Quaternion.identity;
    }

    public void PushCharacter( Vector2 pushDirection, float time )
    {
        if( m_IsAttacking == true )
        {
            GetComponentInChildren<CharacterAnimationListener>().OnAttackFinished();
        }

        m_PushDirection = pushDirection;
        m_PushTime = time;
    }

    public ItemType GetItemThatIsBeingPickedUp()
    {
        return m_PickingUpObject;
    }

    public ItemType GetEquippedShield()
    {
        return m_EquippedShield;
    }

    public ItemType GetEquippedWeapon()
    {
        return m_EquippedWeapon;
    }

    public bool CanAttack()
    {
        if( m_IsAttacking == true )
        {
            return false;
        }

        if( m_EquippedWeapon == ItemType.None )
        {
            return false;
        }

        if( IsBeingPushed() == true )
        {
            return false;
        }

        return true;
    }

    public void DoAttack()
    {
        
    }

    public void OnAttackStarted()
    {
        m_IsAttacking = true;
    }

    public void OnAttackFinished()
    {
        m_IsAttacking = false;
    }
}
