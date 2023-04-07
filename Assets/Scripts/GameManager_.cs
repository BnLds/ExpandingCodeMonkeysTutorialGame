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

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
        CountdownToRestart
    }

    private State state;
    private float countdownToStartTimer = 3f;
    private float countdownToRestartTimer;
    private float countdownToRestartTimerMax = 3f;

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
        TogglePauseMenu();
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

            case State.CountdownToRestart:
                countdownToRestartTimer -= Time.deltaTime;
                
                if(countdownToRestartTimer < 0)
                {
                    state = State.GamePlaying;
                
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            
            case State.GameOver:
                break;
        }
    }

    private void PlayerLoader_OnPlayerInstantiationCompleted(object sender, EventArgs e)
    {
        InputUser.onChange += InputUser_OnChange;
    }

    private void InputUser_OnChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
    {
        if(inputUserChange == InputUserChange.DeviceLost)
        {
            TogglePauseDeviceRemoved();

            OnDeviceLost?.Invoke(this, new EventArgsOnDeviceLost
            {
                deviceName = inputDevice.name
            });

            inputUserChanged = inputUser;

            deviceRemovedUI.OnRemovePlayer += DeviceRemovedUI_OnRemovePlayer;
        }

        if(inputUserChange == InputUserChange.DeviceRegained && inputUserChanged == inputUser)
        {
            countdownToRestartTimer = countdownToRestartTimerMax;
            TogglePauseDeviceRemoved();
            state = State.CountdownToRestart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);

            deviceRemovedUI.OnRemovePlayer -= DeviceRemovedUI_OnRemovePlayer;
        }
    }

    private void DeviceRemovedUI_OnRemovePlayer(object sender, EventArgs e)
    {
        int playerParametersIndex = Array.FindIndex(GameControlsManager.Instance.GetAllControlSchemeParameters(), parameters => parameters.controlScheme == inputUserChanged.controlScheme);
        GameObject playerInstance = GameControlsManager.Instance.GetAllControlSchemeParameters()[playerParametersIndex].playerInstance;
        Destroy(playerInstance);

        countdownToRestartTimer = countdownToRestartTimerMax;
        TogglePauseDeviceRemoved();
        state = State.CountdownToRestart;
        OnStateChanged?.Invoke(this, EventArgs.Empty);

        deviceRemovedUI.OnRemovePlayer -= DeviceRemovedUI_OnRemovePlayer;
    }

    public bool IsGamePlaying()
    {
        return (state == State.GamePlaying);
    }

    public bool IsCountdownToStartActive()
    {
        return (state == State.CountdownToStart);
    }

    public bool IsCountdownToRestartActive()
    {
        return (state == State.CountdownToRestart);
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    public float GetCountdownToRestartTimer()
    {
        return countdownToRestartTimer;
    }

    public bool IsGameOver()
    {
        return(state == State.GameOver);
    }

    public float GetGamePlayingTimerNormalized()
    {
        return (1 - gamePlayingTimer/gamePlayingTimerMax);
    }

    public void TogglePauseMenu()
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

    public void TogglePauseDeviceRemoved()
    {
        isGamePaused = !isGamePaused;
        if(isGamePaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
