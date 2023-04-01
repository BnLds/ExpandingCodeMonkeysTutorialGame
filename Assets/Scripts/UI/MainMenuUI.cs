using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;




    private void Awake()
    {
        playButton.onClick.AddListener(() => 
        {
            //Click
            Loader.Load(Loader.Scene.LobbyScene);
        });

        quitButton.onClick.AddListener(() => 
        {
            //Quit
            Application.Quit();

        });

        Time.timeScale = 1f;
        
    }

}
