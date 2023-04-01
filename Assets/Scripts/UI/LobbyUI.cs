using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private CharacterSelectionSingleUI characterSelectionTemplate;

    private SkinAvailability[] allSkinAvailability;
    private List<CharacterSelectionSingleUI> players;
    private int numberOfPlayersReady;
    private float countdownNumber;


    private void Awake()
    {
        Instance = this;

        countdownNumber = 0f;
        numberOfPlayersReady = 0;
        players = new List<CharacterSelectionSingleUI>();
        players.Add(characterSelectionTemplate);

        allSkinAvailability = new SkinAvailability[skins.characterSkinSOList.Count];
        for (int i = 0; i < skins.characterSkinSOList.Count; i++)
        {
            allSkinAvailability[i].skinName = skins.characterSkinSOList[i].skinName;
            allSkinAvailability[i].isAvailable = true;
        }
    }

    private void Start()
    {
        characterSelectionTemplate.OnPlayerReady += characterSelectionTemplate_OnPlayerReady;
        characterSelectionTemplate.OnPlayerNotReady += characterSelectionTemplate_OnPlayerNotReady;
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

    private void characterSelectionTemplate_OnPlayerReady(object sender, System.EventArgs e)
    {
        numberOfPlayersReady++;
    }

    private void characterSelectionTemplate_OnPlayerNotReady(object sender, System.EventArgs e)
    {
        numberOfPlayersReady--;

    }

    public float GetLobbyCountdown()
    {
        return countdownNumber;
    }









}
