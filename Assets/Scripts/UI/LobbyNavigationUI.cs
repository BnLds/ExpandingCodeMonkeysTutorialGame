using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LobbyNavigationUI : MonoBehaviour
{
    public static event EventHandler OnLobbyNavigationLoaded;
    public static void ResetStaticData()
    {
        OnLobbyNavigationLoaded = null;
    }

    public static LobbyNavigationUI Instance { get; private set;}

    [SerializeField] private Transform container;

    private int numberOfDirectChildren;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        LobbyUI.Instance.OnUIChanged += LobbyUI_OnUIChanged;

        EventSystem eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(container.GetChild(0).GetComponent<LobbyElementsNavigationUI>().GetFirstSelected().gameObject);
    }

    private void LobbyUI_OnUIChanged(object sender, System.EventArgs e)
    {
        numberOfDirectChildren = container.childCount;
        OnLobbyNavigationLoaded?.Invoke(this, EventArgs.Empty);
    }

    public Transform GetNextSibling(int siblingIndex)
    {
        int nextSiblingIndex = (siblingIndex+1) % numberOfDirectChildren;
        return container.GetChild(nextSiblingIndex).gameObject.activeSelf ? container.GetChild(nextSiblingIndex) : GetNextSibling(nextSiblingIndex);        
    }

    public Transform GetPreviousSibling(int siblingIndex)
    {
        int previousSiblingIndex = siblingIndex > 0 ? siblingIndex - 1 : numberOfDirectChildren - 1;
        return container.GetChild(previousSiblingIndex).gameObject.activeSelf ? container.GetChild(previousSiblingIndex) : GetPreviousSibling(previousSiblingIndex);
    }
}
