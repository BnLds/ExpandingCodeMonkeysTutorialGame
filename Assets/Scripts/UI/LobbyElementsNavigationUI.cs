using UnityEngine;
using UnityEngine.UI;

public class LobbyElementsNavigationUI : MonoBehaviour
{
    [SerializeField] protected Selectable firstSelected;

    protected Selectable defaultSelectOnRight_RightSideSelectable()
    {
        return LobbyNavigationUI.Instance.GetNextSibling(transform.GetSiblingIndex()).GetComponent<LobbyElementsNavigationUI>().GetFirstSelected();
    }

    protected Selectable defaultSelectOnLeft_LeftSideSelectable()
    {
        return LobbyNavigationUI.Instance.GetPreviousSibling(transform.GetSiblingIndex()).GetComponent<LobbyElementsNavigationUI>().GetFirstSelected();
    }

    public Selectable GetFirstSelected()
    {
        return firstSelected;
    }
}
