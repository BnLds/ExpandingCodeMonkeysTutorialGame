using UnityEngine;
using System.Collections.Generic;

public static class ExtensionMethods 
{
    private const string HEAD_NAME = "Head";
    private const string BODY_NAME = "Body";


    public static GameObject InstantiatePlayer(GameObject playerPrefab, Vector3 position, Quaternion rotation, PlayerInputActions playerInputActions, Material material)
    {
        GameObject player = GameObject.Instantiate(playerPrefab, position, rotation);
        
        // Set skin
        foreach(MeshRenderer meshRenderer in player.GetComponentsInChildren<MeshRenderer>())
        {
            if(meshRenderer.transform.name == HEAD_NAME || meshRenderer.transform.name == BODY_NAME)
            {
                meshRenderer.material = material;
            }
        }

        //Set PlayerInputActions
        player.GetComponent<PlayerController>().SetPlayerInputActions(playerInputActions);

        return player;
    }
}
