using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform playerPrefab;

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
                InstantiatePlayerExtensionMethod.InstantiatePlayer(playerPrefab.gameObject, Vector3.zero, Quaternion.identity, allControlSchemesParameters[i].playerInputActions);
            }
        }
    }
}
