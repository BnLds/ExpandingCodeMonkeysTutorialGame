using System;
using UnityEngine;
using UnityEngine.UI;

public class NewPlayerSingleNavigationUI : LobbyElementsNavigationUI
{
    [SerializeField] private Button addNewPlayerButton;

    private void Start()
    {
        LobbyNavigationUI.OnLobbyNavigationLoaded += LobbyNavigationUI_OnLobbyNavigationLoaded;
    }

    private void OnDisable()
    {
        LobbyNavigationUI.OnLobbyNavigationLoaded -= LobbyNavigationUI_OnLobbyNavigationLoaded;
    }

    private void LobbyNavigationUI_OnLobbyNavigationLoaded(object sender, EventArgs e)
    {
        addNewPlayerButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = null,
            selectOnLeft = defaultSelectOnLeft_LeftSideSelectable(),
            selectOnRight = defaultSelectOnRight_RightSideSelectable(),
            selectOnUp = null
        };
    }
}
