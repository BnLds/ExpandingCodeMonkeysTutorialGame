using UnityEngine;
using UnityEngine.UI;
using System;

public class AddNewPlayerScreenUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform controlOptionButton;
    [SerializeField] private Button resumeButton;

    public event EventHandler<EventArgsOnControlOptionSelection> OnControlOptionSelection;
    public class EventArgsOnControlOptionSelection : EventArgs
    {
        public string controlSchemeSelected;
    }

    private void Awake()
    {
        resumeButton.onClick.AddListener(() => Hide());
    }

    private void Start()
    {
        PlayerManager.Instance.OnAddNewPlayer += PlayerManager_OnAddNewPlayer;

        Show();

        foreach(Transform child in container)
        {
            child.GetComponent<Button>().onClick.AddListener(() => SelectControlOption(child.GetComponent<ControlOptionSingleButtonUI>().GetControlOption()));
        }
        
    }

    private void PlayerManager_OnAddNewPlayer(object sender, EventArgs e)
    {
        Show();

    }

    private void UpdateVisual()
    {
        foreach(Transform child in container)
        {
            if(child == controlOptionButton)
            {
                continue;
            }
            else
            {
                Destroy(child.gameObject);
                Debug.Log(child + "destroyed");
            }
        }

        if (GameInput.Instance.GetAvailableControlSchemes() != null)
        {

            for (int i = 0; i < GameInput.Instance.GetAvailableControlSchemes().Count-1; i++)
            {
                Instantiate(controlOptionButton, container);
            }
            
            string[] yetToAssignControlScheme = new string[GameInput.Instance.GetAvailableControlSchemes().Count];
            for(int i = 0; i < GameInput.Instance.GetAvailableControlSchemes().Count; i++)
            {
                yetToAssignControlScheme[i] = GameInput.Instance.GetAvailableControlSchemes()[i];
            }

            int e = yetToAssignControlScheme.Length-1;
            foreach(Transform child in container)
            {
                Debug.Log(child);
                child.GetComponent<ControlOptionSingleButtonUI>().SetControlOptionText(yetToAssignControlScheme[e]);
                e--;
                Debug.Log(e);
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

    private void Show()
    {
        GameManager_.Instance.TogglePauseGame();
        UpdateVisual();
        gameObject.SetActive(true);
    }
}
