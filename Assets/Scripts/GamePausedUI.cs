using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePausedUI : MonoBehaviour
{
   
   private void Start()
   {
        GameManager_.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager_.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;
        
        Hide();

   }

    private void Show()
    {
        gameObject.SetActive(true);
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
