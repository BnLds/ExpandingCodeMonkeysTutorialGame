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
        GameManager_.Instance.OnDeviceRegained += GameManager_OnDeviceRegained;
        GameManager_.Instance.OnPlayerDestroyed += GameManager_OnPlayerDestroyed;

        Hide();
    }

    private void GameManager_OnDeviceRegained(object sender, EventArgs e)
    {
        Time.timeScale = 1f;
        Hide();
    }

    private void GameManager_OnPlayerDestroyed(object sender, EventArgs e)
    {
        Time.timeScale = 1f;
        Hide();
    }

    private void GameManager_OnDeviceLost(object sender, GameManager_.EventArgsOnDeviceLost e)
    {
        Show();
        Time.timeScale = 0f;

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
