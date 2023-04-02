using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;



public class CharacterSelectionSingleUI : MonoBehaviour
{

    private const string READY_TEXT = "Ready!";
    private const string NOTREADY_TEXT = "Ready?";


    [SerializeField] private Button nextSkinButton;
    [SerializeField] private Button previousSkinButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI readyButtonText;
    [SerializeField] private RawImage characterRawImage;
    [SerializeField] private Button nextControlButton;
    [SerializeField] private Button previousControlButton;
    [SerializeField] private TextMeshProUGUI controlOptionText;

    public enum State
    {
        AbleToSelect,
        UnableToSelect,
        Ready
    }
  

    public event EventHandler<EventArgsOnPlayerReady> OnPlayerReady;
    public class EventArgsOnPlayerReady : EventArgs
    {
        public Transform origin;
        public int currentSkinDisplayedIndex;
        public string controlOptionSelected;
    }
    public event EventHandler<EventArgsOnPlayerNotReady> OnPlayerNotReady;
    public class EventArgsOnPlayerNotReady : EventArgs
    {
        public Transform origin;
        public int currentSkinDisplayedIndex;
        public string controlOptionSelected;
    }


    private int currentSkinDisplayedIndex;
    private int currentOptionIndexDisplayed;
    private string currentControlOptionSelected;
    private State state;
    private bool isStateUnableToSelectInitialized;
    private bool isStateAbleToSelectInitialized;
    private List<string> availableControls;



    private void Awake()
    {
        currentOptionIndexDisplayed = 0;

        nextControlButton.onClick.AddListener(ShowNextControlOption);
        previousControlButton.onClick.AddListener(ShowPreviousControlOption);

        isStateUnableToSelectInitialized = false;
        isStateAbleToSelectInitialized = false;

        nextSkinButton.onClick.AddListener(ShowNextSkin);
        previousSkinButton.onClick.AddListener(ShowPreviousSkin);
        readyButton.onClick.AddListener(TogglePlayerReady);

    }

    private void Start()
    {
        LobbyUI.Instance.OnSkinLocked += LobbyUI_OnSkinLocked;
        LobbyUI.Instance.OnControlOptionLocked += LobbyUI_OnControlOptionLocked;
        LobbyUI.Instance.OnControlOptionUnlocked += LobbyUI_OnControlOptionUnlocked;

        SkinAvailability[] allSkinsAvailability = LobbyUI.Instance.GetAllSkinsAvailability();

        SkinAvailability firstAvailableSkin = allSkinsAvailability.First(skin => skin.isAvailable == true);
        currentSkinDisplayedIndex =  Array.IndexOf(allSkinsAvailability, firstAvailableSkin);

        //allSkinsAvailability and LobbyUI.Instance.GetSkinsSO() have matching indeces 
        characterRawImage.texture = LobbyUI.Instance.GetSkinsSO()[currentSkinDisplayedIndex].texture;

        availableControls = new List<string> (GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices());

        state = State.AbleToSelect;
    }

    private void Update()
    {
        switch(state)
        {
            case(State.AbleToSelect):

                if (availableControls.Count != 0)
                {
                    if(!isStateAbleToSelectInitialized) InitializeAbleToSelectState();
                }
                else
                {
                    isStateAbleToSelectInitialized = false;
                    state = State.UnableToSelect;
                }
                break;
            
            case(State.UnableToSelect):

                if(availableControls.Count == 0)
                {
                    if(!isStateUnableToSelectInitialized) InitializeUnableToSelectState(); 
                }
                else
                {
                    isStateUnableToSelectInitialized = false;
                    state = State.AbleToSelect;
                }
                break;

            case(State.Ready):
                break;
        }
    }

    private void TogglePlayerReady()
    {
        if(state == State.AbleToSelect)
        {
            isStateAbleToSelectInitialized = false;

            state = State.Ready;

            readyButtonText.text = READY_TEXT;
            readyButtonText.color = Color.green;
            DisableArrowInteractions();

            OnPlayerReady?.Invoke(this, new EventArgsOnPlayerReady
            {
                origin = this.transform,
                currentSkinDisplayedIndex = currentSkinDisplayedIndex,
                controlOptionSelected = currentControlOptionSelected
            });
        }
        else if (state == State.Ready)
        {
            //Move from Ready state to Able to Select
            state = State.AbleToSelect;
            
            OnPlayerNotReady?.Invoke(this, new EventArgsOnPlayerNotReady
            {
                origin = this.transform,
                currentSkinDisplayedIndex = currentSkinDisplayedIndex,
                controlOptionSelected = currentControlOptionSelected
            });
        }
    }

    private void InitializeAbleToSelectState()
    {
        UpdateControlOptionUI();
        ResetReadyButton();
        EnableArrowInteractions();
        isStateAbleToSelectInitialized = true;
    }

    private void InitializeUnableToSelectState()
    {
        DisableArrowInteractions();
        readyButton.interactable = false;
        readyButtonText.text = "Unable to start";
        readyButtonText.color = Color.red;
        controlOptionText.text = "No more control options available!";
        previousControlButton.gameObject.SetActive(false);
        nextControlButton.gameObject.SetActive(false);
        isStateUnableToSelectInitialized = true;
    }

    private void LobbyUI_OnControlOptionLocked(object sender, LobbyUI.EventArgsOnControlOptionLocked e)
    {
        availableControls = new List<string> (GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices());

        if(e.origin != this.transform && currentControlOptionSelected == e.selectedControlName)
        {
            if(availableControls.Count != 0)
            {
                ShowNextControlOption();
            }
        }
        else
        {
            currentOptionIndexDisplayed = availableControls.IndexOf(currentControlOptionSelected);
        }
    }

    private void LobbyUI_OnControlOptionUnlocked(object sender, LobbyUI.EventArgsOnControlOptionUnlocked e)
    {
        //Locked control option is now unlocked. availbleControls list needs to be updated
        availableControls = new List<string> (GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices());

        if(state == State.UnableToSelect || e.origin == this.transform)
        {
            currentControlOptionSelected = e.unselectedControlName;
        }
        
        currentOptionIndexDisplayed = availableControls.IndexOf(currentControlOptionSelected);
    }

    private void LobbyUI_OnSkinLocked(object sender, LobbyUI.EventArgsOnSkinLocked e)
    {
        if(currentSkinDisplayedIndex == e.skinLockedIndex && e.origin != this.transform)
        {
            ShowNextSkin();
        }
    }


    private void ResetReadyButton()
    {
        readyButton.interactable = true;
        readyButtonText.text = NOTREADY_TEXT;
        readyButtonText.color = Color.yellow;
    }

    private void DisableArrowInteractions()
    {
        previousControlButton.interactable = false;
        nextControlButton.interactable = false;
        previousSkinButton.interactable = false;
        nextSkinButton.interactable = false;
    }

    private void EnableArrowInteractions()
    {
        previousControlButton.gameObject.SetActive(true);
        nextControlButton.gameObject.SetActive(true);
        previousControlButton.interactable = true;
        nextControlButton.interactable = true;
        previousSkinButton.interactable = true;
        nextSkinButton.interactable = true;
    }

    private void ShowNextSkin()
    {
        SkinAvailability[] allSkinsAvailability = LobbyUI.Instance.GetAllSkinsAvailability();

        for (int i = 0; i < allSkinsAvailability.Length; i++)
        {
            int j = (currentSkinDisplayedIndex + i + 1) % allSkinsAvailability.Length;
            if(allSkinsAvailability[j].isAvailable)
            {
                currentSkinDisplayedIndex = j;
                break;
            }
        }

        characterRawImage.texture = LobbyUI.Instance.GetSkinsSO()[currentSkinDisplayedIndex].texture;
    }

    private void ShowPreviousSkin()
    {
        SkinAvailability[] allSkinsAvailability = LobbyUI.Instance.GetAllSkinsAvailability();

        for (int i = 0; i < allSkinsAvailability.Length; i++)
        {
            int j = currentSkinDisplayedIndex - i - 1;
            if(j<0) j += allSkinsAvailability.Length;

            if(allSkinsAvailability[j].isAvailable)
            {
                currentSkinDisplayedIndex = j;
                break;
            }
        }
        characterRawImage.texture = LobbyUI.Instance.GetSkinsSO()[currentSkinDisplayedIndex].texture;
    }

    private void ShowNextControlOption()
    {
        currentOptionIndexDisplayed = (currentOptionIndexDisplayed + 1) % availableControls.Count;
        UpdateControlOptionUI();
    }

    private void ShowPreviousControlOption()
    {
        if(currentOptionIndexDisplayed > 0)
        {
            currentOptionIndexDisplayed--;
        }
        else
        {
            currentOptionIndexDisplayed = availableControls.Count;
            currentOptionIndexDisplayed--;
        }

        UpdateControlOptionUI();
    }

    private void UpdateControlOptionUI()
    {
        currentControlOptionSelected = availableControls[currentOptionIndexDisplayed];
        controlOptionText.text = currentControlOptionSelected;
    }
}
