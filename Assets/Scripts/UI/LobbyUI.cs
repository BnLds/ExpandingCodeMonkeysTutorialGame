using System.Collections;
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

    private SkinAvailability[] allSkinAvailability;

    private void Awake()
    {
        Instance = this;

        allSkinAvailability = new SkinAvailability[skins.characterSkinSOList.Count];
        for (int i = 0; i < skins.characterSkinSOList.Count; i++)
        {
            allSkinAvailability[i].skinName = skins.characterSkinSOList[i].skinName;
            allSkinAvailability[i].isAvailable = true;
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




}
