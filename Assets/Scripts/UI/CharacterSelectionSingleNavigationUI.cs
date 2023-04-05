using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionSingleNavigationUI : LobbyElementsNavigationUI
{
    [SerializeField] private Button nextSkinButton;
    [SerializeField] private Button previousSkinButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button nextControlButton;
    [SerializeField] private Button previousControlButton;
    [SerializeField] private Button removePlayerButton;

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
        Selectable selectOnUp_NextSkinButton = readyButton;
        Selectable selectOnDown_ReadyButton = nextSkinButton;
        if(removePlayerButton.gameObject.activeSelf)
        {
            Debug.Log("active");
            selectOnUp_NextSkinButton = removePlayerButton;
            selectOnDown_ReadyButton = removePlayerButton;
        }

        //Navigation is a struct so it is necessary to create a new Navigation() for each button to set it to Explicit
        nextSkinButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = nextControlButton,
            selectOnLeft = previousSkinButton,
            selectOnRight = defaultSelectOnRight_RightSideSelectable(),
            selectOnUp = selectOnUp_NextSkinButton
        };

        previousSkinButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = previousControlButton,
            selectOnLeft = defaultSelectOnLeft_LeftSideSelectable(),
            selectOnRight = nextSkinButton,
            selectOnUp = selectOnUp_NextSkinButton
        };

        nextControlButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = readyButton,
            selectOnLeft = previousControlButton,
            selectOnRight = defaultSelectOnRight_RightSideSelectable(),
            selectOnUp = nextSkinButton
        };

        previousControlButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = readyButton,
            selectOnLeft = defaultSelectOnLeft_LeftSideSelectable(),
            selectOnRight = nextControlButton,
            selectOnUp = previousSkinButton
        };

        readyButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = selectOnDown_ReadyButton,
            selectOnLeft = defaultSelectOnLeft_LeftSideSelectable(),
            selectOnRight = defaultSelectOnRight_RightSideSelectable(),
            selectOnUp = nextControlButton
        };

        removePlayerButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = nextSkinButton,
            selectOnLeft = previousSkinButton,
            selectOnRight = nextSkinButton,
            selectOnUp = readyButton
        };
    }
}
