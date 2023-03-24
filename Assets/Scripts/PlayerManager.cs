
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Transform playerPrefab;

    private float spawnTimerPlayer2 = 7f;

    private bool canSpawnPlayer2 = true;

    private void Start()
    {
        GameObject newPlayer = Instantiate(playerPrefab.gameObject, Vector3.zero, Quaternion.identity);
    }


    private void Update()
    {
        spawnTimerPlayer2 -= Time.deltaTime;

        if(spawnTimerPlayer2 <= 0f && canSpawnPlayer2)
        {
            canSpawnPlayer2 = !canSpawnPlayer2;
            Debug.Log("Attempting to spawn player 2...");

            GameObject newPlayer = Instantiate(playerPrefab.gameObject, Vector3.zero, Quaternion.identity);

            Debug.Log("Player 2 spawned ");
        }

    }



}
