using TMPro;
using UnityEngine;

public class LobbyCountdownUI : MonoBehaviour
{
    private const string NUMBER_POPUP = "NumberPopUp";

    [SerializeField] private TextMeshProUGUI countdownText;

    private Animator animator;
    private int previousCountdownNumber;
    private float countdownNumber;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        Hide();

    }

    private void Update()
    {
        countdownNumber = LobbyUI.Instance.GetLobbyCountdown();
        int intCountdownNumber = Mathf.CeilToInt(countdownNumber);
        countdownText.text = intCountdownNumber.ToString();


        if(previousCountdownNumber != intCountdownNumber)
        {
            previousCountdownNumber = intCountdownNumber;
            animator.SetTrigger(NUMBER_POPUP);
            SoundManager.Instance.PlayCountdownSound();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

