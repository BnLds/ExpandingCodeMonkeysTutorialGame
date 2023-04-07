using System;
using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    private const string NUMBER_POPUP = "NumberPopUp";

    [SerializeField] private TextMeshProUGUI countdownText;

    private Animator animator;
    private int previousCountdownNumber;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameManager_.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void OnDestroy()
    {
        GameManager_.Instance.OnStateChanged -= GameManager_OnStateChanged;
    }

    private void Update()
    {
        int countdownNumber;
        if(GameManager_.Instance.IsCountdownToStartActive())
        {
            countdownNumber = Mathf.CeilToInt(GameManager_.Instance.GetCountdownToStartTimer());
        }
        else
        {
            countdownNumber = Mathf.CeilToInt(GameManager_.Instance.GetCountdownToRestartTimer());
        }
        countdownText.text = countdownNumber.ToString();

        if(previousCountdownNumber != countdownNumber)
        {
            previousCountdownNumber = countdownNumber;
            animator.SetTrigger(NUMBER_POPUP);
            SoundManager.Instance.PlayCountdownSound();
        }
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if(GameManager_.Instance.IsCountdownToStartActive() || GameManager_.Instance.IsCountdownToRestartActive())
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
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
