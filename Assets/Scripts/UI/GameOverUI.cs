using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipesDeliveredText; 
    [SerializeField] private TextMeshProUGUI totalSalesValueText; 


    private void Start()
    {
        GameManager_.Instance.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }




    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if(GameManager_.Instance.IsGameOver())
        {
            Show();
            
            recipesDeliveredText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();
            totalSalesValueText.text = "$" + DeliveryManager.Instance.GetTotalSalesValue().ToString();

        }
        else
        {
            Hide();
        }
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
