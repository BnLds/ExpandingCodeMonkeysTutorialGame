using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DeviceRemovedUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deviceRemovedText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button removePlayerButton;

    public event EventHandler OnRemovePlayer;

    private void Awake()
    {
        
        mainMenuButton.onClick.AddListener(() => Loader.Load(Loader.Scene.MainMenuScene));
        removePlayerButton.onClick.AddListener(RemovePlayer);
    }

    private void Start()
    {
        GameManager_.Instance.OnDeviceLost += GameManager_OnDeviceLost;
        GameManager_.Instance.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if(GameManager_.Instance.IsCountdownToRestartActive())
        {
            Hide();
        }
    }

    private void GameManager_OnDeviceLost(object sender, GameManager_.EventArgsOnDeviceLost e)
    {
        Show();
        deviceRemovedText.text = e.deviceName;
    }

    private void RemovePlayer()
    {
        OnRemovePlayer?.Invoke(this, EventArgs.Empty);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
