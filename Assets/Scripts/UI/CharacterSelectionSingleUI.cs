using System;
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
        public int currentSkinDisplayedIndex;
        public string controlOptionSelected;
    }


    private int currentSkinDisplayedIndex;
    private bool isReady;
    private int currentOptionIndexDisplayed;
    private string currentControlOptionSelected;
    private static bool isAbleToSetReady;

    private void Awake()
    {
        currentOptionIndexDisplayed = 0;

        nextControlButton.onClick.AddListener(ShowNextControlOption);
        previousControlButton.onClick.AddListener(ShowPreviousControlOption);

        isReady = false;
        isAbleToSetReady = true;

        nextSkinButton.onClick.AddListener(ShowNextSkin);
        previousSkinButton.onClick.AddListener(ShowPreviousSkin);
        readyButton.onClick.AddListener(TogglePlayerReady);

        ResetReadyButton();

    }

    private void Start()
    {
        LobbyUI.Instance.OnSkinLocked += LobbyUI_OnSkinLocked;
        LobbyUI.Instance.OnControlOptionSelected += LobbyUI_OnControlOptionLocked;

        SkinAvailability[] allSkinsAvailability = LobbyUI.Instance.GetAllSkinsAvailability();

        SkinAvailability firstAvailableSkin = allSkinsAvailability.First(skin => skin.isAvailable == true);
        currentSkinDisplayedIndex =  Array.IndexOf(allSkinsAvailability, firstAvailableSkin);

        //allSkinsAvailability and LobbyUI.Instance.GetSkinsSO() have matching indeces 
        characterRawImage.texture = LobbyUI.Instance.GetSkinsSO()[currentSkinDisplayedIndex].texture;

        UpdateControlOptionUI();
    }

    private void Update()
    {
        bool hasNoMoreControlsForRemainingPlayers = LobbyUI.Instance.GetNumberOfPlayersNotReady() != 0 && !IsAvailableControlNotNull();
        isAbleToSetReady = !(hasNoMoreControlsForRemainingPlayers);
        readyButton.interactable = isAbleToSetReady || isReady;
    }

    private void LobbyUI_OnControlOptionLocked(object sender, LobbyUI.EventArgsOnControlOptionSelected e)
    {
        if(e.origin != this.transform && currentControlOptionSelected == e.selectedControlName)
        {
            ShowNextControlOption();
        }
    }

    private void LobbyUI_OnSkinLocked(object sender, LobbyUI.EventArgsOnSkinLocked e)
    {
        if(currentSkinDisplayedIndex == e.skinLockedIndex && e.origin != this.transform)
        {
            ShowNextSkin();
        }
    }

    private void TogglePlayerReady()
    {

        isReady = !isReady;
        if(isReady)
        {
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
        else
        {
            ResetReadyButton();
            EnableArrowInteractions();

            OnPlayerNotReady?.Invoke(this, new EventArgsOnPlayerNotReady
            {
                currentSkinDisplayedIndex = currentSkinDisplayedIndex,
                controlOptionSelected = currentControlOptionSelected
            });

        }
    }

    private void ResetReadyButton()
    {
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
        if (IsAvailableControlNotNull())
        {
            currentOptionIndexDisplayed = (currentOptionIndexDisplayed + 1) % GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices().Count;
        }

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
            currentOptionIndexDisplayed = GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices().Count;
            currentOptionIndexDisplayed--;
        }

        UpdateControlOptionUI();
    }

    private void UpdateControlOptionUI()
    {
        if(IsAvailableControlNotNull())
        {
            currentControlOptionSelected = GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices()[currentOptionIndexDisplayed];
            controlOptionText.text = currentControlOptionSelected;
            previousControlButton.gameObject.SetActive(true);
            previousControlButton.gameObject.SetActive(true);
        }
        else
        {
            controlOptionText.text = "No more control options available!";
            previousControlButton.gameObject.SetActive(false);
            nextControlButton.gameObject.SetActive(false);
        }
    }

    private bool IsAvailableControlNotNull()
    {
        return GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices().Count != 0;
    }
    

}
