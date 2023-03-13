using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    private float movementSpeed = 7f;

    private bool isWalking;
    private void Update()
    {
        #region Variables
        bool isKeyUpPressed = Input.GetKey(KeyCode.W);
        bool isKeyDownPressed = Input.GetKey(KeyCode.S);
        bool isKeyLeftPressed = Input.GetKey(KeyCode.A);
        bool isKeyRightPressed = Input.GetKey(KeyCode.D);

        Vector2 inputVector2 = new Vector2(0,0);
        #endregion

        if (isKeyUpPressed)
        {
            inputVector2.y = +1;
        }
        if(isKeyDownPressed)
        {
            inputVector2.y = -1;
        }
        if(isKeyLeftPressed)
        {
            inputVector2.x = -1;
        }
        if(isKeyRightPressed)
        {
            inputVector2.x = +1;
        }

        inputVector2 = inputVector2.normalized;

        Vector3 moveDirection = new Vector3(inputVector2.x, 0, inputVector2.y);
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
        isWalking = moveDirection != Vector3.zero;
 
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward,moveDirection, Time.deltaTime * rotateSpeed);

    }

    public bool IsWalking()
    {
        return isWalking;
    }
}
