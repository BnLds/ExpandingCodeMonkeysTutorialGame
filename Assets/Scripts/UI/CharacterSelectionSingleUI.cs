using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;



public class CharacterSelectionSingleUI : MonoBehaviour
{
    [SerializeField] private Button rightButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI readyButtonText;
    [SerializeField] private RawImage characterRawImage;

    private int currentSkinDisplayedIndex;

    private void Awake()
    {
        rightButton.onClick.AddListener(ShowNextSkin);
        leftButton.onClick.AddListener(ShowPreviousSkin);
        //readyButton.onClick.AddListener();

    }

    private void Start()
    {
        SkinAvailability[] allSkinsAvailability = LobbyUI.Instance.GetAllSkinsAvailability();

        SkinAvailability firstAvailableSkin = allSkinsAvailability.First(skin => skin.isAvailable == true);
        currentSkinDisplayedIndex =  Array.IndexOf(allSkinsAvailability, firstAvailableSkin);

        //allSkinsAvailability and LobbyUI.Instance.GetSkinsSO() have matching indeces 
        characterRawImage.texture = LobbyUI.Instance.GetSkinsSO()[currentSkinDisplayedIndex].texture;
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
