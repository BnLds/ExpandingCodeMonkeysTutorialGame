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
    [SerializeField] private TextMeshProUGUI connectControlText;

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

    private void Awake()
    {
        currentOptionIndexDisplayed = 0;

        rightControlButton.onClick.AddListener(ShowNextControlOption);
        leftControlButton.onClick.AddListener(ShowPreviousControlOption);

        isReady = false;

        rightSkinButton.onClick.AddListener(ShowNextSkin);
        leftSkinButton.onClick.AddListener(ShowPreviousSkin);
        readyButton.onClick.AddListener(TogglePlayerReady);

    }

    private void Start()
    {
        LobbyUI.Instance.OnSkinLocked += LobbyUI_OnSkinLocked;
        LobbyUI.Instance.OnControlOptionSelected += LobbyUI_OnControlOptionSelected;

        SkinAvailability[] allSkinsAvailability = LobbyUI.Instance.GetAllSkinsAvailability();

        SkinAvailability firstAvailableSkin = allSkinsAvailability.First(skin => skin.isAvailable == true);
        currentSkinDisplayedIndex =  Array.IndexOf(allSkinsAvailability, firstAvailableSkin);

        //allSkinsAvailability and LobbyUI.Instance.GetSkinsSO() have matching indeces 
        characterRawImage.texture = LobbyUI.Instance.GetSkinsSO()[currentSkinDisplayedIndex].texture;

        UpdateControlOptionText();
    }

    private void LobbyUI_OnControlOptionSelected(object sender, LobbyUI.EventArgsOnControlOptionSelected e)
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

            OnPlayerReady?.Invoke(this, new EventArgsOnPlayerReady
            {
                origin = this.transform,
                currentSkinDisplayedIndex = currentSkinDisplayedIndex,
                controlOptionSelected = currentControlOptionSelected
            });

        }
        else
        {
            readyButtonText.text = NOTREADY_TEXT;
            readyButtonText.color = Color.yellow;

            OnPlayerNotReady?.Invoke(this, new EventArgsOnPlayerNotReady
            {
                currentSkinDisplayedIndex = currentSkinDisplayedIndex,
                controlOptionSelected = currentControlOptionSelected
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

    private void ShowNextControlOption()
    {
        currentOptionIndexDisplayed = (currentOptionIndexDisplayed + 1) % GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices().Count;
        UpdateControlOptionText();
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

        UpdateControlOptionText();

    }

    private void UpdateControlOptionText()
    {
        if(GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices().Count == 0)
        {
            DisplayConnectDevices();
            controlOptionText.text = "No more control options available!";
        }
        else
        {
            currentControlOptionSelected = GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices()[currentOptionIndexDisplayed];
            controlOptionText.text = currentControlOptionSelected;
        }
    }

    private void DisplayConnectDevices()
    {
        //Notify player more controls are available
        if(GameControlsManager.Instance.GetSupportedDevicesNotConnected() == null)
        {
            connectControlText.gameObject.SetActive(false);
        }
        else if(GameControlsManager.Instance.GetSupportedDevicesNotConnected().Count == 1)
        {
            connectControlText.gameObject.SetActive(true);
            string deviceName = GameControlsManager.Instance.GetSupportedDevicesNotConnected()[0];
            connectControlText.text = "Connect a "+ deviceName + " to enable "+ deviceName +" controls.";

        }
        else
        {
            connectControlText.gameObject.SetActive(true);
            string deviceNames = GameControlsManager.Instance.GetSupportedDevicesNotConnected()[0];
            for (int i = 1; i<GameControlsManager.Instance.GetSupportedDevicesNotConnected().Count; i++)
            {
                deviceNames += " or " + GameControlsManager.Instance.GetSupportedDevicesNotConnected()[i];
            }
            connectControlText.text = "Connect a "+ deviceNames + " to enable "+ deviceNames +" controls."; 
        }
    }



}
