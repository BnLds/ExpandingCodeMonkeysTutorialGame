using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private Button addNewPlayerButton;
    [SerializeField] private AddNewPlayerScreenUI addNewPlayerScreenUI;

    public event EventHandler OnAddNewPlayer;

    private void Awake()
    {
        Instance = this;

        addNewPlayerButton.onClick.AddListener(() => OnAddNewPlayer?.Invoke(this, EventArgs.Empty));
    }

    private void Start()
    {
        addNewPlayerScreenUI.OnControlOptionSelection += AddNewPlayerScreenUI_OnControlOptionSelection;

    }

    private void AddNewPlayerScreenUI_OnControlOptionSelection(object sender, AddNewPlayerScreenUI.EventArgsOnControlOptionSelection e)
    {
        GameInput.Instance.SetNextPlayerControlScheme(e.controlSchemeSelected);
        Instantiate(playerPrefab.gameObject, Vector3.zero, Quaternion.identity);
    }

}
