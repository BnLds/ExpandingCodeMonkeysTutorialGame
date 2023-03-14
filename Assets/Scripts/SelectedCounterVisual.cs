using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVis : MonoBehaviour
{

    [SerializeField] private ClearCounter clearCounter;
    [SerializeField] private GameObject visualGameObject;

    private void Start() 
    {
        PlayerController.Instance.OnSelectedCounterChanged += PlayerController_OnSelectedCounterChanged;
    }

    private void PlayerController_OnSelectedCounterChanged(object sender, PlayerController.OnSelectedCounterChangedEventArgs e)
    {
        if(e.selectedCounter == clearCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        visualGameObject.SetActive(true);
    }

    private void Hide()
    {
        visualGameObject.SetActive(false);
    }
}
