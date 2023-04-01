using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;



public class CharacterSelectionSingleUI : MonoBehaviour
{

    private const string READY_TEXT = "Ready!";
    private const string NOTREADY_TEXT = "Ready?";


    [SerializeField] private Button rightSkinButton;
    [SerializeField] private Button leftSkinButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI readyButtonText;
    [SerializeField] private RawImage characterRawImage;
    [SerializeField] private Button rightControlButton;
    [SerializeField] private Button leftControlButton;
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
    }


    private int currentSkinDisplayedIndex;
    private bool isReady;
    private int currentOptionIndexDisplayed;

    private void Awake()
    {
        currentOptionIndexDisplayed = 0;

        rightControlButton.onClick.AddListener(DisplayNextControlOption);
        leftControlButton.onClick.AddListener(DisplayPreviousControlOption);

        isReady = false;

        rightSkinButton.onClick.AddListener(ShowNextSkin);
        leftSkinButton.onClick.AddListener(ShowPreviousSkin);
        readyButton.onClick.AddListener(TogglePlayerReady);

    }

    private void Start()
    {
        LobbyUI.Instance.OnSkinLocked += LobbyUI_OnSkinLocked;

        SkinAvailability[] allSkinsAvailability = LobbyUI.Instance.GetAllSkinsAvailability();

        SkinAvailability firstAvailableSkin = allSkinsAvailability.First(skin => skin.isAvailable == true);
        currentSkinDisplayedIndex =  Array.IndexOf(allSkinsAvailability, firstAvailableSkin);

        //allSkinsAvailability and LobbyUI.Instance.GetSkinsSO() have matching indeces 
        characterRawImage.texture = LobbyUI.Instance.GetSkinsSO()[currentSkinDisplayedIndex].texture;

        UpdateControlOptionText();
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
            OnPlayerReady?.Invoke(this, new EventArgsOnPlayerReady
            {
                origin = this.transform,
                currentSkinDisplayedIndex = currentSkinDisplayedIndex,
                controlOptionSelected = GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices()[currentOptionIndexDisplayed]
            });
        }
        else
        {
            readyButtonText.text = NOTREADY_TEXT;
            readyButtonText.color = Color.yellow;
            OnPlayerNotReady?.Invoke(this, new EventArgsOnPlayerNotReady
            {
                currentSkinDisplayedIndex = currentSkinDisplayedIndex
            });
        }
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

    private void DisplayNextControlOption()
    {
        currentOptionIndexDisplayed = (currentOptionIndexDisplayed + 1) % GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices().Count;
        UpdateControlOptionText();
    }

    private void DisplayPreviousControlOption()
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

        UpdateControlOptionText();

    }

    private void UpdateControlOptionText()
    {
        controlOptionText.text = GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices()[currentOptionIndexDisplayed];
    }



}
