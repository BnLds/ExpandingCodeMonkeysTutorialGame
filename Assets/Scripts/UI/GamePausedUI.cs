using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GamePausedUI : MonoBehaviour
{
   [SerializeField] private Button resumeButton;
   [SerializeField] private Button mainMenuButton;
   [SerializeField] private Button optionsButton;


    private void Awake()
   {
        mainMenuButton.onClick.AddListener(() => 
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        resumeButton.onClick.AddListener(() => 
        {
            GameManager_.Instance.TogglePauseMenu();
        });

        optionsButton.onClick.AddListener(() => 
        {
            Hide();
            OptionsUI.Instance.Show(Show);
        });

   }

   private void Start()
   {
        GameManager_.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager_.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;

        Hide();
   }

   

    private void Show()
    {
        gameObject.SetActive(true);

        resumeButton.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void GameManager_OnGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }
    private void GameManager_OnGameUnpaused(object sender, System.EventArgs e)
    {
        Hide();
    }
}
