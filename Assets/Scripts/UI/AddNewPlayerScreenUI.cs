using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class AddNewPlayerScreenUI : MonoBehaviour
{
    public static AddNewPlayerScreenUI Instance { get; private set; }

    [SerializeField] private Transform container;
    [SerializeField] private Transform buttonTemplate;
    [SerializeField] private Button resumeButton;
    [SerializeField] private TextMeshProUGUI connectControlText;
    [SerializeField] private Transform maxPlayerReachedScreen;
    [SerializeField] private Button resumeButtonMaxPlayerReached;



    public event EventHandler<EventArgsOnControlOptionSelection> OnControlOptionSelection;
    public class EventArgsOnControlOptionSelection : EventArgs
    {
        public string controlSchemeSelected;
    }

    private void Awake()
    {
        Instance = this;
        resumeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        GameManager_.Instance.TogglePauseMenu();

        UpdateVisual();        
    }

    private void UpdateVisual()
    {
        if(GameControlsManager.Instance.IsNumberOfPlayersMaxReached())
        {
            maxPlayerReachedScreen.gameObject.SetActive(true);
            resumeButtonMaxPlayerReached.onClick.RemoveAllListeners();
            resumeButtonMaxPlayerReached.onClick.AddListener(Hide);
        }
        else
        {
            maxPlayerReachedScreen.gameObject.SetActive(false);
            StartCoroutine(UpdateVisualCoroutine());
        }
    }

    private IEnumerator UpdateVisualCoroutine()
    {
        DeleteButtons();

        // wait 1 frame for completion of Destroy method in DeleteButtons()
        yield return new WaitForEndOfFrame();

        List<string> availableControlSchemes = GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices();
        bool isControlSchemeAvailable = availableControlSchemes.Count != 0;

        if (isControlSchemeAvailable)
        {
            int numberOfControlSchemesAvailable = availableControlSchemes.Count;
            int numberOfNewButtonsToInstantiate = numberOfControlSchemesAvailable - 1;

            for (int i = 0; i < numberOfNewButtonsToInstantiate; i++)
            {
                Instantiate(buttonTemplate, container);
            }         

            int availableControlSchemesIndex = numberOfControlSchemesAvailable-1;
            foreach(Transform child in container)
            {
                child.GetComponent<ControlOptionSingleButtonUI>().SetControlOptionText(availableControlSchemes[availableControlSchemesIndex]);
                availableControlSchemesIndex--;
            }

            AddListenerToNewButtons();
            UpdateMessageToPlayerToConnectDevices();
        }
        else
        {
            buttonTemplate.GetComponent<ControlOptionSingleButtonUI>().SetControlOptionText("There is no additional control option available.");
            UpdateMessageToPlayerToConnectDevices();
        }
        yield return null;
    }

    private void UpdateMessageToPlayerToConnectDevices()
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

    private void AddListenerToNewButtons()
    {
        foreach(Transform child in container)
        {
            child.GetComponent<Button>().onClick.AddListener(() => SelectControlOption(child.GetComponent<ControlOptionSingleButtonUI>().GetControlOption()));
        }   
    }

    private void RemoveListenerToButtons()
    {
        foreach(Transform child in container)
        {
            child.GetComponent<Button>().onClick.RemoveAllListeners();
        }  
    }

    private void DeleteButtons()
    {
        RemoveListenerToButtons();
        foreach(Transform child in container)
        {
            if(child == buttonTemplate)
            {
                continue;
            }
            else
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void SelectControlOption(string controlSchemeSelected)
    {
        OnControlOptionSelection?.Invoke(this, new EventArgsOnControlOptionSelection
            {
                controlSchemeSelected = controlSchemeSelected
            });

        Hide();
    }


    private void Hide()
    {
        GameManager_.Instance.TogglePauseMenu();
        gameObject.SetActive(false);
    }

    public void ShowAndUpdateVisual()
    {
        gameObject.SetActive(true);
        GameManager_.Instance.TogglePauseMenu();
        UpdateVisual();
    }
}
