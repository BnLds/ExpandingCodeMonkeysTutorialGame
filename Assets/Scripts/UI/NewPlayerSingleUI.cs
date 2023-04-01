using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NewPlayerSingleUI : MonoBehaviour
{
    [SerializeField] private Button addNewPlayerButton;
    [SerializeField] private Button rightArrow;
    [SerializeField] private Button leftArrow;
    [SerializeField] private TextMeshProUGUI controlOptionText;

    public event EventHandler<EventArgsOnAddNewPlayer> OnAddNewPlayer;
    public class EventArgsOnAddNewPlayer : EventArgs
    {
        public Transform transform;
        public string controlOptionSelected;
    }

    private int currentOptionIndexDisplayed;

    private void Awake()
    {
        currentOptionIndexDisplayed = 0;

        rightArrow.onClick.AddListener(DisplayNextControlOption);
        leftArrow.onClick.AddListener(DisplayPreviousControlOption);
        addNewPlayerButton.onClick.AddListener(() => OnAddNewPlayer?.Invoke(this, new EventArgsOnAddNewPlayer
        {
            transform = this.transform,
            controlOptionSelected = GameControlsManager.Instance.GetAvailableControlSchemesWithConnectedDevices()[currentOptionIndexDisplayed]
        }));
    }

    private void Start()
    {
        UpdateControlOptionText();
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
