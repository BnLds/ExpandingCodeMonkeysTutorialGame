using UnityEngine;
using System;

public class PlayerLoader : MonoBehaviour
{
    public static PlayerLoader Instance { get; private set; }

    public static event EventHandler OnPlayerInstantiationCompleted;
    public static void ResetStaticData()
    {
        OnPlayerInstantiationCompleted = null;
    }

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InstantiatePlayers();
    }

    private void InstantiatePlayers()
    {
        GameControlsManager.Instance.GeneratePlayerInputActions();
        ControlSchemeParameters[] allControlSchemesParameters = GameControlsManager.Instance.GetAllControlSchemeParameters();
        for (int i = 0; i < allControlSchemesParameters.Length; i++)
        {
            if(allControlSchemesParameters[i].playerInputActions != null)
            {
                GameObject playerInstance = ExtensionMethods.InstantiatePlayer(playerPrefab.gameObject, spawnPoints[i].position, spawnPoints[i].rotation, allControlSchemesParameters[i].playerInputActions, allControlSchemesParameters[i].playerVisualMaterial);
                allControlSchemesParameters[i].playerInstance = playerInstance;
            }
        }

        OnPlayerInstantiationCompleted?.Invoke(this, EventArgs.Empty);
    }
}
