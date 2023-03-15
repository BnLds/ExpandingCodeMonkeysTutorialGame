using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(PlayerController player)
    {
        if(!HasKitchenObject())
        {
            //There is no kitchen object here
            if(player.HasKitchenObject())
            {
                //Player is carrying something
                player.GetKitchenObject().SetKitchenObjectParent(this);  
            }
            else
            {
                //player not carrying anything
            }
        }
        else
        {
            //There is a kitchen object here
            if(player.HasKitchenObject())
            {
                //Player is carrying something
            }
            else
            {
                //player is not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

}
