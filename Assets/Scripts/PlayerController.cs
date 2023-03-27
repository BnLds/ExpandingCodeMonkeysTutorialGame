using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IKitchenObjectParent
{
    public event EventHandler OnPickedSomething;

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    


    private bool isWalking;
    private Vector3 lastInteractDirection;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;
    private PlayerInputActions playerInputActions;

    private void Start() 
    {
        playerInputActions = GameInput.Instance.SetPlayerControlScheme();

        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void OnDisable()
    {
        GameInput.Instance.DestroyPlayerInputActions(playerInputActions);
    }

    private void GameInput_OnInteractAction(object sender, GameInput.OnInteractActionEventArgs e)
    {
        if(!GameManager_.Instance.IsGamePlaying()) return;

        if(selectedCounter != null && GameInput.Instance.isActionMine(e.action, playerInputActions))
        {
            selectedCounter.Interact(this);
        }
    }
    private void GameInput_OnInteractAlternateAction(object sender, GameInput.OnInteractAlternateActionEventArgs e)
    {
        if(!GameManager_.Instance.IsGamePlaying()) return;

        if(selectedCounter != null && GameInput.Instance.isActionMine(e.action, playerInputActions))
        {
            selectedCounter.InteractAlternate(this);
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
        Vector2 inputVector2Normalized = GameInput.Instance.GetMovementVectorNormalized(playerInputActions);
        Vector3 moveDirection = new Vector3(inputVector2Normalized.x, 0, inputVector2Normalized.y);

        if(moveDirection != Vector3.zero)
        {
            lastInteractDirection = moveDirection;
        }

        float interactDistance = 2f;
        if(Physics.Raycast(transform.position, lastInteractDirection, out RaycastHit raycastHit,interactDistance, countersLayerMask))
        {
            if(raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                //Has ClearCounter
                if(baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
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
        Vector2 inputVector2Normalized = GameInput.Instance.GetMovementVectorNormalized(playerInputActions);
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
            //Test if there is an input on x greater than .5f so it doesn't switch to diagonal moves too quickly with gamepads 
            canMove = (moveDirection.x < - .5f || moveDirection.x > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight ,playerRadius, moveDirectionX, moveDistance);

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
                //Test if there is an input on z greater than .5f so it doesn't switch to diagonal moves too quickly with gamepads 
                canMove = (moveDirection.z < - .5f || moveDirection.z > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight ,playerRadius, moveDirectionZ, moveDistance);

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

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
        selectedCounter = selectedCounter
        });

    }
    

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }


    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }


}
