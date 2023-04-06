using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public struct SkinAvailability
{
    public string skinName;
    public bool isAvailable;

    public SkinAvailability(string skinName, bool isAvailable)
    {
        this.skinName = skinName;
        this.isAvailable = isAvailable;
    }
}


public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] private CharacterSkinsListSO skins;
    [SerializeField] private LobbyCountdownUI lobbyCountdownUI;
    [SerializeField] private CharacterSelectionSingleUI characterSelectionUITemplate;
    [SerializeField] private NewPlayerSingleUI newPlayerUITemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI connectControlText;

    public event EventHandler<EventArgsOnSkinLocked> OnSkinLocked;
    public class EventArgsOnSkinLocked : EventArgs
    {
        public int skinLockedIndex;
        public Transform origin;
    }

    public event EventHandler<EventArgsOnControlOptionLocked> OnControlOptionSelected;
    public class EventArgsOnControlOptionLocked : EventArgs
    {
        public string selectedControlName;
        public Transform origin;
    }

    public event EventHandler<EventArgsOnControlOptionUnlocked> OnControlOptionUnselected;
    public class EventArgsOnControlOptionUnlocked
    {
        public string unselectedControlName;
        public Transform origin;
    }

    public event EventHandler<EventArgsOnCharacterParametersSelected> OnCharacterParametersSelected;
    public class EventArgsOnCharacterParametersSelected : EventArgs
    {
        public string selectedControlName;
        public Material selectedSkinMaterial;
    }

    public event EventHandler<EventArgsOnCharacterParametersUnselected> OnCharacterParametersUnselected;
    public class EventArgsOnCharacterParametersUnselected : EventArgs
    {
        public string unselectedControlName;
    }

    public event EventHandler OnUIChanged;

    private SkinAvailability[] allSkinAvailability;
    private List<CharacterSelectionSingleUI> players;
    private List<NewPlayerSingleUI> newPlayerUIs;
    private int numberOfPlayersReady;
    private float countdownNumber;


    private void Awake()
    {
        Instance = this;

        countdownNumber = 0f;
        numberOfPlayersReady = 0;
        players = new List<CharacterSelectionSingleUI>();
        players.Add(characterSelectionUITemplate);

        newPlayerUIs = new List<NewPlayerSingleUI>();
        newPlayerUIs.Add(newPlayerUITemplate);

        allSkinAvailability = new SkinAvailability[skins.characterSkinSOList.Count];
        for (int i = 0; i < skins.characterSkinSOList.Count; i++)
        {
            allSkinAvailability[i].skinName = skins.characterSkinSOList[i].skinName;
            allSkinAvailability[i].isAvailable = true;
        }
    }

    private void Start()
    {
        characterSelectionUITemplate.OnPlayerReady += characterSelectionTemplate_OnPlayerReady;
        characterSelectionUITemplate.OnPlayerNotReady += characterSelectionTemplate_OnPlayerNotReady;

        DisplayConnectDevices();
        InstantiateNewPlayerUIOnLoad();

        //Invoking an event in Start can result in null reference, so we wait 1 frame before invoking
        StartCoroutine(InvokeCoroutine());
    }

    private void InstantiateNewPlayerUIOnLoad()
    {
        int numberOfNewPlayerUIToInstantiate = GameControlsManager.Instance.GetMaxNumberOfPlayers() - 1;

        if(numberOfNewPlayerUIToInstantiate > 1)
        {
            int remaining = numberOfNewPlayerUIToInstantiate;

            while(remaining > 1)
            {
                NewPlayerSingleUI newPlayerUI = Instantiate(newPlayerUITemplate, container);
                newPlayerUIs.Add(newPlayerUI);
                remaining--;
            }
            
        }
        else
        {
            newPlayerUITemplate.gameObject.SetActive(false);
        }

        foreach(NewPlayerSingleUI newPlayerUI in newPlayerUIs)
        {
            newPlayerUI.OnAddNewPlayer += NewPlayerUI_OnAddNewPlayer;
        }     
    }

    private IEnumerator InvokeCoroutine()
    {
        yield return new WaitForEndOfFrame();
        OnUIChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NewPlayerUI_OnAddNewPlayer(object sender, NewPlayerSingleUI.EventArgsOnAddNewPlayer e)
    {
        Transform characterSelectionUI = Instantiate(characterSelectionUITemplate.transform, container);

        players.Add(characterSelectionUI.GetComponent<CharacterSelectionSingleUI>());
        int indexInLayerGroup = players.Count - 1;
        characterSelectionUI.SetSiblingIndex(indexInLayerGroup);

        characterSelectionUI.GetComponent<CharacterSelectionSingleUI>().ShowRemovePlayerButton();
        characterSelectionUI.GetComponent<CharacterSelectionSingleUI>().OnPlayerReady += characterSelectionTemplate_OnPlayerReady;
        characterSelectionUI.GetComponent<CharacterSelectionSingleUI>().OnPlayerNotReady += characterSelectionTemplate_OnPlayerNotReady;
        characterSelectionUI.GetComponent<CharacterSelectionSingleUI>().OnRemovePlayer += characterSelectionTemplate_OnRemovePlayer;


        e.transform.GetComponent<NewPlayerSingleUI>().OnAddNewPlayer -= NewPlayerUI_OnAddNewPlayer;
        if(e.transform == newPlayerUITemplate.transform)
        {
            e.transform.gameObject.SetActive(false);
        }
        else
        {
            Destroy(e.transform.gameObject);
        }

        // Wait 1 frame for the Destroy to act before firing event
        StartCoroutine(InvokeCoroutine());
    }

    private void Update()
    {
        if(numberOfPlayersReady == players.Count)
        {
            countdownNumber += Time.deltaTime;
            lobbyCountdownUI.Show();
        }
        else
        {
            lobbyCountdownUI.Hide();
            countdownNumber = 0f;
        }

        if(countdownNumber >= 2.5f)
        {
            Loader.Load(Loader.Scene.GameScene);
        }
    }


    public SkinAvailability[] GetAllSkinsAvailability()
    {
        return allSkinAvailability;
    }

    public List<CharacterSkinSO> GetSkinsSO()
    {
        return skins.characterSkinSOList;
    }

    private void characterSelectionTemplate_OnPlayerReady(object sender, CharacterSelectionSingleUI.EventArgsOnPlayerReady e)
    {
        allSkinAvailability[e.currentSkinDisplayedIndex].isAvailable = false;      

        OnSkinLocked?.Invoke(this, new EventArgsOnSkinLocked
        {
            skinLockedIndex = e.currentSkinDisplayedIndex,
            origin = e.origin
        });

        OnCharacterParametersSelected?.Invoke(this, new EventArgsOnCharacterParametersSelected
        {
            selectedControlName = e.controlOptionSelected,
            selectedSkinMaterial = skins.characterSkinSOList[e.currentSkinDisplayedIndex].Material
        });

        OnControlOptionSelected?.Invoke(this, new EventArgsOnControlOptionLocked
        {
            selectedControlName = e.controlOptionSelected,
            origin = e.origin
        });

        numberOfPlayersReady++;
    }

    private void characterSelectionTemplate_OnPlayerNotReady(object sender, CharacterSelectionSingleUI.EventArgsOnPlayerNotReady e)
    {
        allSkinAvailability[e.currentSkinDisplayedIndex].isAvailable = true;

        OnCharacterParametersUnselected?.Invoke(this, new EventArgsOnCharacterParametersUnselected
        {
            unselectedControlName = e.controlOptionSelected,
        });

        OnControlOptionUnselected?.Invoke(this, new EventArgsOnControlOptionUnlocked
        {
            unselectedControlName = e.controlOptionSelected,
            origin = e.origin
        });

        numberOfPlayersReady--;
    }

    private void characterSelectionTemplate_OnRemovePlayer(object sender, EventArgs e)
    {
        CharacterSelectionSingleUI characterSelectionSingleUI = sender as CharacterSelectionSingleUI;
        players.Remove(characterSelectionSingleUI.GetComponent<CharacterSelectionSingleUI>());
        characterSelectionSingleUI.GetComponent<CharacterSelectionSingleUI>().OnPlayerReady -= characterSelectionTemplate_OnPlayerReady;
        characterSelectionSingleUI.GetComponent<CharacterSelectionSingleUI>().OnPlayerNotReady -= characterSelectionTemplate_OnPlayerNotReady;
        characterSelectionSingleUI.GetComponent<CharacterSelectionSingleUI>().OnRemovePlayer -= characterSelectionTemplate_OnRemovePlayer;

        Destroy(characterSelectionSingleUI.gameObject);

        Transform newPlayerUI = Instantiate(newPlayerUITemplate.transform, container);
        newPlayerUI.SetAsLastSibling();
        newPlayerUI.gameObject.SetActive(true);
        newPlayerUIs.Add(newPlayerUI.GetComponent<NewPlayerSingleUI>());
        newPlayerUI.GetComponent<NewPlayerSingleUI>().OnAddNewPlayer += NewPlayerUI_OnAddNewPlayer;

        // Wait 1 frame for the Destroy to act before firing event
        StartCoroutine(InvokeCoroutine());
    }

    private void DisplayConnectDevices()
    {
        //Notify player more controls are available
        if (GameControlsManager.Instance.GetSupportedDevicesNotConnected().Count == 0)
        {
            connectControlText.gameObject.SetActive(false);
        }
        else if (GameControlsManager.Instance.GetSupportedDevicesNotConnected().Count == 1)
        {
            connectControlText.gameObject.SetActive(true);
            string deviceName = GameControlsManager.Instance.GetSupportedDevicesNotConnected()[0];
            connectControlText.text = "Connect a " + deviceName + " to enable " + deviceName + " controls.";

        }
        else
        {
            connectControlText.gameObject.SetActive(true);
            string deviceNames = GameControlsManager.Instance.GetSupportedDevicesNotConnected()[0];
            for (int i = 1; i < GameControlsManager.Instance.GetSupportedDevicesNotConnected().Count; i++)
            {
                deviceNames += " or " + GameControlsManager.Instance.GetSupportedDevicesNotConnected()[i];
            }
            connectControlText.text = "Connect a " + deviceNames + " to enable " + deviceNames + " controls.";
        }
    }

    public float GetLobbyCountdown()
    {
        return countdownNumber;
    }

    public int GetNumberOfPlayersNotReady()
    {
        return players.Count - numberOfPlayersReady;
    }


}
