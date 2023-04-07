using System;
using UnityEngine;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem;

public class GameManager_ : MonoBehaviour
{

    public static GameManager_ Instance {get; private set;}

    [SerializeField] private DeviceRemovedUI deviceRemovedUI;

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler<EventArgsOnDeviceLost> OnDeviceLost;
    public class EventArgsOnDeviceLost : EventArgs
    {
        public string deviceName;
    }
    public event EventHandler OnPlayerDestroyed;
    public event EventHandler OnDeviceRegained;


    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State state;
    private float countdownToStartTimer = 3f;
    [SerializeField] private float gamePlayingTimerMax = 60f;
    private float gamePlayingTimer;
    private bool isGamePaused = false;
    private InputUser inputUserChanged;



    private void Awake()
    {
        Instance = this;

        state = State.WaitingToStart;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        PlayerLoader.OnPlayerInstantiationCompleted += PlayerLoader_OnPlayerInstantiationCompleted;
    }

    private void PlayerLoader_OnPlayerInstantiationCompleted(object sender, EventArgs e)
    {
        InputUser.onChange += InputUser_OnChange;
    }

    private void InputUser_OnChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
    {
        Debug.Log(inputUser.controlScheme);
        Debug.Log(inputUserChange);
        Debug.Log(inputDevice);

        if(inputUserChange == InputUserChange.DeviceLost)
        {
            OnDeviceLost?.Invoke(this, new EventArgsOnDeviceLost
            {
                deviceName = inputDevice.name
            });

            inputUserChanged = inputUser;

            deviceRemovedUI.OnRemovePlayer += DeviceRemovedUI_OnRemovePlayer;
        }

        if(inputUserChange == InputUserChange.DeviceRegained && inputUserChanged == inputUser)
        {
            OnDeviceRegained?.Invoke(this, EventArgs.Empty);

            deviceRemovedUI.OnRemovePlayer -= DeviceRemovedUI_OnRemovePlayer;
        }
    }

    private void DeviceRemovedUI_OnRemovePlayer(object sender, EventArgs e)
    {
        int playerParametersIndex = Array.FindIndex(GameControlsManager.Instance.GetAllControlSchemeParameters(), parameters => parameters.controlScheme == inputUserChanged.controlScheme);
        GameObject playerInstance = GameControlsManager.Instance.GetAllControlSchemeParameters()[playerParametersIndex].playerInstance;
        Destroy(playerInstance);
        OnPlayerDestroyed?.Invoke(this, EventArgs.Empty);

        deviceRemovedUI.OnRemovePlayer -= DeviceRemovedUI_OnRemovePlayer;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if(state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                break;

            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                
                if(countdownToStartTimer < 0)
                {
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);

                }
                break;

            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                
                if(gamePlayingTimer < 0)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);

                }
                break;
            
            case State.GameOver:
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return (state == State.GamePlaying);
    }

    public bool IsCountdownToStartActive()
    {
        return (state == State.CountdownToStart);
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    public bool IsGameOver()
    {
        return(state == State.GameOver);
    }

    public float GetGamePlayingTimerNormalized()
    {
        return (1 - gamePlayingTimer/gamePlayingTimerMax);
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        if(isGamePaused)
        {
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
        Time.timeScale = 1f;
        OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

}
