using UnityEngine;
using UnityEngine.UI;
using System;

public class NewPlayerSingleUI : MonoBehaviour
{
    [SerializeField] private Button addNewPlayerButton;


    public event EventHandler<EventArgsOnAddNewPlayer> OnAddNewPlayer;
    public class EventArgsOnAddNewPlayer : EventArgs
    {
        public Transform transform;
    }

    

    private void Awake()
    {
        addNewPlayerButton.onClick.AddListener(() => OnAddNewPlayer?.Invoke(this, new EventArgsOnAddNewPlayer
        {
            transform = this.transform
        }));
    }
}
