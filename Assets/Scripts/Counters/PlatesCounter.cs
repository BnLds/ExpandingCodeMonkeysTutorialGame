using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    private float spawnPlateTimer = 4f;
    private float spawnPlateTimerMax = 4f;
    private int plateSpawnedAmount;
    private int platesSpawnedAmountMax = 4;


    private void Update()
    {
        if(GameManager_.Instance.IsGamePlaying())
        {
            spawnPlateTimer += Time.deltaTime;
            if(spawnPlateTimer > spawnPlateTimerMax)
            {
                spawnPlateTimer = 0f;

                if(plateSpawnedAmount < platesSpawnedAmountMax)
                {
                    plateSpawnedAmount ++;

                    OnPlateSpawned?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
    }

    private void GameManager_OnGameStart(object sender, EventArgs e)
    {
        spawnPlateTimer = 4f;
    }

    public override void Interact(PlayerController player)
    {
        if(!player.HasKitchenObject())
        {
            //player is empty handed
            if(plateSpawnedAmount > 0)
            {
                //there is at least one plate here
                plateSpawnedAmount--;

                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);

                OnPlateRemoved?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
