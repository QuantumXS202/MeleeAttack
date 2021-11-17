using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using StateMachine.PlayerInputHandlers;

public class MeleeAttack : Ability
{
    [SerializeField] private Vector3 hitArea = new Vector3(1, 1, 0);
    [SerializeField] private Vector3 hitAreaOffset = new Vector3(1,0,0);
    [SerializeField] private int direction = 1;
    [SerializeField] private float fireRate = 0.75f;
    
    private bool _isOnCoolDown = false;
    
    [Header("UnityEvents")]
    [SerializeField] protected UnityEvent onHitRight = new UnityEvent();
    [SerializeField] protected UnityEvent onHitLeft = new UnityEvent();
    
    [SerializeField] protected UnityEvent onUseAbilityRight = new UnityEvent();
    [SerializeField] protected UnityEvent onUseAbilityLeft = new UnityEvent();
    

    public override void Use(InputAction.CallbackContext context)
    {
        if (_isOnCoolDown) return;
        _isOnCoolDown = true;
        StartCoroutine(MeleeHit());
        
        onUseAbility?.Invoke();
        Direction = GetLookDirection();
        
        if (direction > 0) onUseAbilityRight.Invoke();
        else if (direction < 0) onUseAbilityLeft.Invoke();

        foreach (GameObject target in GetTargetsInRange())
        {
            IHitable hitable = target.GetComponent<IHitable>();
            if(hitable == null) return;
            OnHitTarget(hitable);
        }
    }

    public int GetLookDirection()
    {
        return transform.localScale.x == 1 ? 1 : -1;
    }
    
    private List<GameObject> GetTargetsInRange()
    {
        List<GameObject> objectsInRange = new List<GameObject>();
        var offset = hitAreaOffset;
        offset.x *= direction;
        
        Collider[] hitColliders = Physics.OverlapBox(transform.position + offset, hitArea / 2);

        foreach (Collider collider in hitColliders)
        {
            if (!IsTargetValid(collider, objectsInRange)) continue;
                
            objectsInRange.Add(collider.gameObject);
        }

        return objectsInRange;
    }

    private bool IsTargetValid(Collider collider, List<GameObject> targets)
    {
        GameObject objectInRange = collider.gameObject;

        return objectInRange != null && IsHitable(objectInRange) && !targets.Contains(objectInRange) && !objectInRange.Equals(gameObject);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        var offset = hitAreaOffset;
        offset.x *= direction;
        Gizmos.DrawWireCube(transform.position + offset,hitArea);
    }
    
    public bool IsHitable(GameObject targetObject)
    {
        return targetObject.GetComponent<IHitable>() != null;
    }

    private void OnHitTarget(IHitable hitable)
    {
        if (direction > 0) onHitRight.Invoke();
        else if (direction < 0) onHitLeft.Invoke();
        hitable.Hit();
    }

    public int Direction
    {
        get => direction;
        set => direction = value;
    }

    public override void OnInput(InputAction.CallbackContext aContext)
    {
        Use(aContext);
        GetComponentInChildren<StateMachine.StateMachine>().SetBool("Attacking", true);
    }

    IEnumerator MeleeHit()
    {
        yield return new WaitForSeconds(fireRate);
        _isOnCoolDown = false;
    }
}


    


