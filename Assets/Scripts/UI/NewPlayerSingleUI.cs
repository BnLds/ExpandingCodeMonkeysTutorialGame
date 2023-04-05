using System;
using UnityEngine;
using UnityEngine.UI;


public class NewPlayerSingleUI : MonoBehaviour
{
    public static event EventHandler OnPlayerAdditon;
    public static void ResetStaticData()
    {
        OnPlayerAdditon = null;
    }

    [SerializeField] private Button addNewPlayerButton;


    public event EventHandler<EventArgsOnAddNewPlayer> OnAddNewPlayer;
    public class EventArgsOnAddNewPlayer : EventArgs
    {
        public Transform transform;
    }

    

    private void Awake()
    {
        addNewPlayerButton.onClick.AddListener(NotifyAddNewPlayer);
        addNewPlayerButton.Select();
    }

    private void NotifyAddNewPlayer()
    {
        OnAddNewPlayer?.Invoke(this, new EventArgsOnAddNewPlayer
        {
            transform = this.transform
        });

        OnPlayerAdditon?.Invoke(this, EventArgs.Empty);
    }
}
