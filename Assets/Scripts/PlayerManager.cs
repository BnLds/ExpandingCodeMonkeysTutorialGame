using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private Button addNewPlayerButton;


    private void Awake()
    {
        Instance = this;

        addNewPlayerButton.onClick.AddListener(AddNewPlayerScreenUI.Instance.ShowAndUpdateVisual);
    }

    private void Start()
    {
        AddNewPlayerScreenUI.Instance.OnControlOptionSelection += AddNewPlayerScreenUI_OnControlOptionSelection;

    }

    private void AddNewPlayerScreenUI_OnControlOptionSelection(object sender, AddNewPlayerScreenUI.EventArgsOnControlOptionSelection e)
    {
        GameControlsManager.Instance.SetNextPlayerControlScheme(e.controlSchemeSelected);
        Instantiate(playerPrefab.gameObject, Vector3.zero, Quaternion.identity);
    }

}
