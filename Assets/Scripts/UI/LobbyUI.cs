using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public event EventHandler<EventArgsOnSkinLocked> OnSkinLocked;
    public class EventArgsOnSkinLocked : EventArgs
    {
        public int skinLockedIndex;
        public Transform origin;
    }

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

    private void NewPlayerUI_OnAddNewPlayer(object sender, NewPlayerSingleUI.EventArgsOnAddNewPlayer e)
    {
        Transform characterSelectionUI = Instantiate(characterSelectionUITemplate.transform, container);

        players.Add(characterSelectionUI.GetComponent<CharacterSelectionSingleUI>());
        int indexInLayerGroup = players.Count - 1;
        characterSelectionUI.SetSiblingIndex(indexInLayerGroup);

        characterSelectionUI.GetComponent<CharacterSelectionSingleUI>().OnPlayerReady += characterSelectionTemplate_OnPlayerReady;
        characterSelectionUI.GetComponent<CharacterSelectionSingleUI>().OnPlayerNotReady += characterSelectionTemplate_OnPlayerNotReady;

        //e.controlOptionSelected

        e.transform.GetComponent<NewPlayerSingleUI>().OnAddNewPlayer -= NewPlayerUI_OnAddNewPlayer;
        e.transform.gameObject.SetActive(false);

        
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

        if(countdownNumber >= 3f)
        {
            Debug.Log("Start!!");
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
        numberOfPlayersReady++;

    }

    private void characterSelectionTemplate_OnPlayerNotReady(object sender, CharacterSelectionSingleUI.EventArgsOnPlayerNotReady e)
    {
        allSkinAvailability[e.currentSkinDisplayedIndex].isAvailable = true;
        numberOfPlayersReady--;

    }

    public float GetLobbyCountdown()
    {
        return countdownNumber;
    }









}