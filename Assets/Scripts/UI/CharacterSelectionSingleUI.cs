using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;



public class CharacterSelectionSingleUI : MonoBehaviour
{

    private const string READY_TEXT = "Ready!";
    private const string NOTREADY_TEXT = "Ready?";


    [SerializeField] private Button rightButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI readyButtonText;
    [SerializeField] private RawImage characterRawImage;

    public event EventHandler OnPlayerReady;
    public event EventHandler OnPlayerNotReady;


    private int currentSkinDisplayedIndex;
    private bool isReady;

    private void Awake()
    {
        isReady = false;

        rightButton.onClick.AddListener(ShowNextSkin);
        leftButton.onClick.AddListener(ShowPreviousSkin);
        readyButton.onClick.AddListener(TogglePlayerReady);

    }

    private void Start()
    {
        SkinAvailability[] allSkinsAvailability = LobbyUI.Instance.GetAllSkinsAvailability();

        SkinAvailability firstAvailableSkin = allSkinsAvailability.First(skin => skin.isAvailable == true);
        currentSkinDisplayedIndex =  Array.IndexOf(allSkinsAvailability, firstAvailableSkin);

        //allSkinsAvailability and LobbyUI.Instance.GetSkinsSO() have matching indeces 
        characterRawImage.texture = LobbyUI.Instance.GetSkinsSO()[currentSkinDisplayedIndex].texture;
    }

    private void TogglePlayerReady()
    {
        isReady = !isReady;
        if(isReady)
        {
            readyButtonText.text = READY_TEXT;
            readyButtonText.color = Color.green;
            OnPlayerReady?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            readyButtonText.text = NOTREADY_TEXT;
            readyButtonText.color = Color.yellow;
            OnPlayerNotReady?.Invoke(this, EventArgs.Empty);
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


}
