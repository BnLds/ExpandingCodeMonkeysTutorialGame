using UnityEngine;

public class ResetStaticDataManager : MonoBehaviour
{
    private void Awake()
    {
        CuttingCounter.ResetStaticData();
        TrashCounter.ResetStaticData();
        BaseCounter.ResetStaticData();
        CharacterSelectionSingleUI.ResetStaticData();
        NewPlayerSingleUI.ResetStaticData();
        LobbyNavigationUI.ResetStaticData();
        PlayerLoader.ResetStaticData();
    }
}
