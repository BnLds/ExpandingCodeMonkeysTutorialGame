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
    [SerializeField] private Transform buttonHolderTemplate;
    [SerializeField] private Button resumeButton;
    [SerializeField] private TextMeshProUGUI connectControlText;

    public event EventHandler<EventArgsOnControlOptionSelection> OnControlOptionSelection;
    public class EventArgsOnControlOptionSelection : EventArgs
    {
        public string controlSchemeSelected;
    }

    private void Awake()
    {
        Instance = this;
        resumeButton.onClick.AddListener(() => Hide());
    }

    private void Start()
    {
        GameManager_.Instance.TogglePauseGame();

        UpdateVisual();        
    }

    private void UpdateVisual()
    {
        StartCoroutine(UpdateVisualCoroutine());
    }

    private IEnumerator UpdateVisualCoroutine()
    {
        DeleteButtons();

        // wait 1 frame for completion of Destroy method in DeleteButtons()
        yield return new WaitForEndOfFrame();

        List<string> availableControlSchemes = GameInput.Instance.GetAvailableControlSchemesWithConnectedDevices();
        bool isControlSchemeAvailable = availableControlSchemes.Count != 0;

        if (isControlSchemeAvailable)
        {
            int numberOfControlSchemesAvailable = availableControlSchemes.Count;
            int numberOfNewButtonsToInstantiate = numberOfControlSchemesAvailable - 1;

            for (int i = 0; i < numberOfNewButtonsToInstantiate; i++)
            {
                Instantiate(buttonHolderTemplate, container);
            }
            
            string[] availableControlSchemesArray = new string[numberOfControlSchemesAvailable];
            for(int i = 0; i < availableControlSchemesArray.Length; i++)
            {
                availableControlSchemesArray[i] = availableControlSchemes[i];
            }

            int availableControlSchemesArrayIndex = availableControlSchemesArray.Length-1;
            foreach(Transform child in container)
            {
                child.GetComponent<ControlOptionSingleButtonUI>().SetControlOptionText(availableControlSchemesArray[availableControlSchemesArrayIndex]);
                availableControlSchemesArrayIndex--;
            }

            AddListenerToNewButtons();

            //Notify player more controls are available
            if(GameInput.Instance.GetSupportedDevicesNotConnected() == null)
            {
                connectControlText.gameObject.SetActive(false);
            }
            else if(GameInput.Instance.GetSupportedDevicesNotConnected().Count == 1)
            {
                connectControlText.gameObject.SetActive(true);
                string deviceName = GameInput.Instance.GetSupportedDevicesNotConnected()[0];
                connectControlText.text = "Connect a "+ deviceName + "to enable "+ deviceName +" controls.";

            }
            else
            {
                connectControlText.gameObject.SetActive(true);
                string deviceNames = GameInput.Instance.GetSupportedDevicesNotConnected()[0];
                for (int i = 1; i<GameInput.Instance.GetSupportedDevicesNotConnected().Count; i++)
                {
                    deviceNames += " or " + GameInput.Instance.GetSupportedDevicesNotConnected()[i];
                }
                connectControlText.text = "Connect a "+ deviceNames + " to enable "+ deviceNames +" controls."; 
            }
        }

        
        yield return null;
    }

    private void AddListenerToNewButtons()
    {
        foreach(Transform child in container)
        {
            child.GetComponent<ControlOptionSingleButtonUI>().GetButtonTemplate().GetComponent<Button>().onClick.AddListener(() => SelectControlOption(child.GetComponent<ControlOptionSingleButtonUI>().GetControlOption()));
        }   
    }

    private void RemoveListenerToButtons()
    {
        foreach(Transform child in container)
        {
            child.GetComponent<ControlOptionSingleButtonUI>().GetButtonTemplate().GetComponent<Button>().onClick.RemoveAllListeners();
        }  
    }

    private void DeleteButtons()
    {
        RemoveListenerToButtons();
        foreach(Transform child in container)
        {
            if(child == buttonHolderTemplate)
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
        GameManager_.Instance.TogglePauseGame();
        gameObject.SetActive(false);
    }

    public void ShowAndUpdateVisual()
    {
        gameObject.SetActive(true);
        GameManager_.Instance.TogglePauseGame();
        UpdateVisual();
    }
}
