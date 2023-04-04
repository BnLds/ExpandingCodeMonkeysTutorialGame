using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatePlayerExtensionMethod 
{
    public static GameObject InstantiatePlayer(GameObject playerPrefab, Vector3 position, Quaternion rotation, PlayerInputActions playerInputActions)
    {
        GameObject player = GameObject.Instantiate(playerPrefab, position, rotation);
        player.GetComponent<PlayerController>().SetPlayerInputActions(playerInputActions);

        return player;
    }
}
