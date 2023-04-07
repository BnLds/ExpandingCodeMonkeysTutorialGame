using UnityEngine;

public class ResetDontDestroyOnLoadData : MonoBehaviour
{
    private void Awake()
    {
        if(GameControlsManager.Instance != null)
        {
            GameControlsManager.Instance.ResetDontDestroyOnLoadData();
        }
    }
}
