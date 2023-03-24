
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Transform playerPrefab;

    private float spawnTimer = 7f;
    private bool canSpawn = true;

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if(spawnTimer<=0f && canSpawn)
        {
            canSpawn = !canSpawn;
            Debug.Log("Attempting to spawn a new player...");

            GameObject newPlayer = Instantiate(playerPrefab.gameObject, Vector3.zero, Quaternion.identity);

            

            Debug.Log("Player spawned ");
        }
    }



}
