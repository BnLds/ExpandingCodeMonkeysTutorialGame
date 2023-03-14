using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance {get; private set;}

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public ClearCounter selectedCounter;
    }

    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private GameInput gameInput; 
    [SerializeField] private LayerMask countersLayerMask;
    


    private bool isWalking;
    private Vector3 lastInteractDirection;
    private ClearCounter selectedCounter;

    private void Awake() 
    {
        if(Instance != null)
        {
            Debug.LogError("There is more than one PlayerController instance");
        }
        Instance = this;    
    }

    private void Start() 
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if(selectedCounter != null)
        {
            selectedCounter.Interact();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteractions()
    {
        Vector2 inputVector2Normalized = gameInput.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVector2Normalized.x, 0, inputVector2Normalized.y);

        if(moveDirection != Vector3.zero)
        {
            lastInteractDirection = moveDirection;
        }

        float interactDistance = 2f;
        if(Physics.Raycast(transform.position, lastInteractDirection, out RaycastHit raycastHit,interactDistance, countersLayerMask))
        {
            if(raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
            {
                //Has ClearCounter
                if(clearCounter != selectedCounter)
                {
                    SetSelectedCounter(clearCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector2Normalized = gameInput.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVector2Normalized.x, 0, inputVector2Normalized.y);

        float moveDistance = movementSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight ,playerRadius, moveDirection, moveDistance);

        if(!canMove)
        {
            //cannot move towards direction

            //attempt only X movement
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight ,playerRadius, moveDirectionX, moveDistance);

            if(canMove)
            {
                //Can only move on the X
                moveDirection = moveDirectionX;
            } 
            else 
            {
                //Cannot move only on the X
                //Attempt only Z movement
                Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight ,playerRadius, moveDirectionZ, moveDistance);

                if(canMove)
                {
                    moveDirection = moveDirectionZ;
                }
                else
                {
                    //cannot move in any direction 
                }

            }
        }

        if (canMove)
        {
            transform.position += moveDirection * movementSpeed * Time.deltaTime;
        }

        isWalking = moveDirection != Vector3.zero;
 
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward,moveDirection, Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(ClearCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
        selectedCounter = selectedCounter
        });

    }
}
