using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;



public class CharacterSelectionSingleUI : MonoBehaviour
{
    //the below events are responsible to let SoundManager know when to play a sound
    public static event EventHandler OnReadySelection;
    public static event EventHandler OnReadyUnselection;
    public static event EventHandler OnClick;
    public static event EventHandler OnPlayerRemoval;
    public static void ResetStaticData()
    {
        OnReadySelection = null;
        OnReadyUnselection = null;
        OnClick = null;
        OnPlayerRemoval = null;
    }

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
    [SerializeField] private Button removePlayerButton;

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

    public event EventHandler OnRemovePlayer;

    private int currentSkinDisplayedIndex;
    private int currentOptionIndexDisplayed;
    private string currentControlOptionDisplayed;
    private State state;
    private bool isStateUnableToSelectInitialized;
    private bool isStateAbleToSelectInitialized;
    private List<string> availableControls;



    private void Awake()
    {
        currentOptionIndexDisplayed = 0;
        isStateUnableToSelectInitialized = false;
        isStateAbleToSelectInitialized = false;
        availableControls = new List<string>();

        nextControlButton.onClick.AddListener(() =>
        {
            ShowNextControlOption();
            OnClick?.Invoke(this, EventArgs.Empty);
        });
        previousControlButton.onClick.AddListener( () =>
        {
            ShowPreviousControlOption();
            OnClick?.Invoke(this, EventArgs.Empty);
        });
        nextSkinButton.onClick.AddListener( () => 
            {
                ShowNextSkin();
                OnClick?.Invoke(this, EventArgs.Empty);
            });
        previousSkinButton.onClick.AddListener( () => 
            {
                ShowPreviousSkin();
                OnClick?.Invoke(this, EventArgs.Empty);
            });
        readyButton.onClick.AddListener(TogglePlayerReady);
        removePlayerButton.onClick.AddListener(ActPlayerRemoval);

    }

    private void Start()
    {
        LobbyUI.Instance.OnSkinLocked += LobbyUI_OnSkinLocked;
        LobbyUI.Instance.OnControlOptionSelected += LobbyUI_OnControlOptionLocked;
        LobbyUI.Instance.OnControlOptionUnselected += LobbyUI_OnControlOptionUnlocked;
        CharacterSelectionSingleUI.OnPlayerRemoval += CharacterSelectionSingleUI_OnPlayerRemoval;
        GameControlsManager.OnAvailableControlsChange += GameControlsManager_OnAvailableControlsChange;

        SkinAvailability[] allSkinsAvailability = LobbyUI.Instance.GetAllSkinsAvailability();

        SkinAvailability firstAvailableSkin = allSkinsAvailability.First(skin => skin.isAvailable == true);
        currentSkinDisplayedIndex =  Array.IndexOf(allSkinsAvailability, firstAvailableSkin);

        //allSkinsAvailability and LobbyUI.Instance.GetSkinsSO() have matching indeces 
        characterRawImage.texture = LobbyUI.Instance.GetSkinsSO()[currentSkinDisplayedIndex].texture;

        UpdateAvailableControls();

        state = State.AbleToSelect;

        //Initialize Ready button to Selected on the EventSystem so it appears selected for gamepad.
        readyButton.Select();
    }

    private void GameControlsManager_OnAvailableControlsChange(object sender, EventArgs e)
    {
        //Update list first in case of new control connected
        UpdateAvailableControls();

        if(state == State.Ready)
        {
            OnPlayerNotReady?.Invoke(this, new EventArgsOnPlayerNotReady
            {
                origin = this.transform,
                currentSkinDisplayedIndex = currentSkinDisplayedIndex,
                controlOptionSelected = currentControlOptionDisplayed
            });
            
            state = State.UnableToSelect;
        }
    }


    private void OnDestroy()
    {
        LobbyUI.Instance.OnSkinLocked -= LobbyUI_OnSkinLocked;
        LobbyUI.Instance.OnControlOptionSelected -= LobbyUI_OnControlOptionLocked;
        LobbyUI.Instance.OnControlOptionUnselected -= LobbyUI_OnControlOptionUnlocked;
        CharacterSelectionSingleUI.OnPlayerRemoval -= CharacterSelectionSingleUI_OnPlayerRemoval;
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
                controlOptionSelected = currentControlOptionDisplayed
            });

            OnReadySelection?.Invoke(this, EventArgs.Empty);
        }
        else if (state == State.Ready)
        {
            //Move from Ready state to Able to Select
            state = State.AbleToSelect;
            
            OnPlayerNotReady?.Invoke(this, new EventArgsOnPlayerNotReady
            {
                origin = this.transform,
                currentSkinDisplayedIndex = currentSkinDisplayedIndex,
                controlOptionSelected = currentControlOptionDisplayed
            });

            OnReadyUnselection?.Invoke(this, EventArgs.Empty);
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
        currentOptionIndexDisplayed = 0;
        controlOptionText.text = "No more control options available!";
        previousControlButton.gameObject.SetActive(false);
        nextControlButton.gameObject.SetActive(false);
        isStateUnableToSelectInitialized = true;
    }

    private void CharacterSelectionSingleUI_OnPlayerRemoval(object sender, EventArgs e)
    {
        if(state == State.Ready)
        {
            TogglePlayerReady();
        }
    }

    private void LobbyUI_OnControlOptionLocked(object sender, LobbyUI.EventArgsOnControlOptionLocked e)
    {
        UpdateAvailableControls();
        if(e.origin != this.transform && currentControlOptionDisplayed == e.selectedControlName)
        {
            if(availableControls.Count != 0)
            {
                ShowNextControlOption();
            }
        }
        else
        {
            currentOptionIndexDisplayed = availableControls.IndexOf(currentControlOptionDisplayed);
        }
    }

    private void LobbyUI_OnControlOptionUnlocked(object sender, LobbyUI.EventArgsOnControlOptionUnlocked e)
    {
        //Locked control option is now unlocked. availbleControls list needs to be updated
        UpdateAvailableControls();
        if(state == State.UnableToSelect || e.origin == this.transform)
        {
            if(availableControls.Contains(e.unselectedControlName))
            {
                currentControlOptionDisplayed = e.unselectedControlName;
            }
            else
            {
                //This situation happens if a control option was removed after the device was disconnected
                ShowNextControlOption();
            }
        }
        
        currentOptionIndexDisplayed = availableControls.IndexOf(currentControlOptionDisplayed);
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
        if(availableControls.Count != 0)
        {
            currentOptionIndexDisplayed = (currentOptionIndexDisplayed + 1) % availableControls.Count;
            UpdateControlOptionUI();
        }
        else
        {
            state = State.UnableToSelect;
        }
        
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
        currentControlOptionDisplayed = availableControls[currentOptionIndexDisplayed];
        controlOptionText.text = currentControlOptionDisplayed;
    }

    private void UpdateAvailableControls()
    {
        availableControls = GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices();
    }

    private void ActPlayerRemoval()
    {
        if(state == State.Ready)
        {
            OnPlayerNotReady?.Invoke(this, new EventArgsOnPlayerNotReady
            {
                origin = this.transform,
                currentSkinDisplayedIndex = currentSkinDisplayedIndex,
                controlOptionSelected = currentControlOptionDisplayed
            });
        }
        
        OnRemovePlayer?.Invoke(this, EventArgs.Empty);
        OnPlayerRemoval?.Invoke(this, EventArgs.Empty);
    }

    public void ShowRemovePlayerButton()
    {
        removePlayerButton.gameObject.SetActive(true);
    }
}
