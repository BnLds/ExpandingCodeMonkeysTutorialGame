using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Linq;

public struct ControlSchemeParameters
{
    public InputControlScheme controlScheme;
    public bool isAvailableForNewPlayer;
    public PlayerInputActions playerInputActions;
    public Material playerVisualMaterial;
    public GameObject playerInstance;


    public ControlSchemeParameters(InputControlScheme controlScheme, bool isAvailableForNewPlayer, PlayerInputActions playerInputActions, Material playerVisualMaterial, GameObject playerInstance)
    {
        this.controlScheme = controlScheme;
        this.isAvailableForNewPlayer = isAvailableForNewPlayer;
        this.playerInputActions = playerInputActions;
        this.playerVisualMaterial = playerVisualMaterial;
        this.playerInstance = playerInstance;
    }
}

public class GameControlsManager : MonoBehaviour
{
    public static GameControlsManager Instance { get; private set; }

    public static event EventHandler OnAvailableControlsChange;
    public static event EventHandler OnControlAddedPlaySound;
    public static event EventHandler OnControlRemovedPlaySound;
    public static void ResetStaticData()
    {
        OnAvailableControlsChange = null;
        OnControlAddedPlaySound = null;
        OnControlRemovedPlaySound = null;
    }


    [SerializeField] private int numberOfPlayersMax = 3;

    private static ControlSchemeParameters[] allControlSchemesParameters;
    private List<InputDevice> connectedDevices;
    private List<string> availableControlSchemesWithConnectedDevices;
    private List<string> supportedDevicesNotConnected;
    private int numberOfPlayers;
    private PlayerInputActions defaultPlayerInputActions;

    public void ResetDontDestroyOnLoadData()
    {
        Destroy(Instance);
    }

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LobbyUI.Instance.OnCharacterParametersSelected += LobbyUI_OnCharacterParametersSelected;
        LobbyUI.Instance.OnCharacterParametersUnselected += LobbyUI_OnCharacterParametersUnselected;

        numberOfPlayers = 0;
        connectedDevices = InputSystem.devices.ToList();

        defaultPlayerInputActions = new PlayerInputActions();

        CreateAllControlSchemesParameters();
    }

    private void Start()
    {
        InputSystem.onDeviceChange += InputSystem_onDeviceChange;
    }

    private void InputSystem_onDeviceChange(InputDevice inputDevice, InputDeviceChange inputDeviceChange)
    {
        switch(inputDeviceChange)
        {
            default:
                break;
            case InputDeviceChange.Added:
                connectedDevices = InputSystem.devices.ToList();
                OnAvailableControlsChange?.Invoke(this, EventArgs.Empty);
                OnControlAddedPlaySound?.Invoke(this, EventArgs.Empty);
                break;
            case InputDeviceChange.Removed:
                connectedDevices = InputSystem.devices.ToList();
                OnAvailableControlsChange?.Invoke(this, EventArgs.Empty);
                OnControlRemovedPlaySound?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    private void LobbyUI_OnCharacterParametersSelected(object sender, LobbyUI.EventArgsOnCharacterParametersSelected e)
    {
        int indexSelectedControl = Array.FindIndex(allControlSchemesParameters, scheme => scheme.controlScheme.bindingGroup == e.selectedControlName);
        allControlSchemesParameters[indexSelectedControl].isAvailableForNewPlayer = false;
        allControlSchemesParameters[indexSelectedControl].playerVisualMaterial = e.selectedSkinMaterial;
    }

    private void LobbyUI_OnCharacterParametersUnselected(object sender, LobbyUI.EventArgsOnCharacterParametersUnselected e)
    {
        int indexSelectedControl = Array.FindIndex(allControlSchemesParameters, scheme => scheme.controlScheme.bindingGroup == e.unselectedControlName);
        allControlSchemesParameters[indexSelectedControl].isAvailableForNewPlayer = true;
    }

    private void CreateAllControlSchemesParameters()
    {
        allControlSchemesParameters = new ControlSchemeParameters[defaultPlayerInputActions.controlSchemes.Count];
        for (int i = 0; i < defaultPlayerInputActions.controlSchemes.Count; i++)
        {
            allControlSchemesParameters[i].controlScheme = defaultPlayerInputActions.controlSchemes[i];
            allControlSchemesParameters[i].isAvailableForNewPlayer = true;
            allControlSchemesParameters[i].playerInputActions = null;
            allControlSchemesParameters[i].playerInstance = null;
        }
    }

    public void GeneratePlayerInputActions()
    {
        for (int i = 0; i < allControlSchemesParameters.Length; i++)
        {
            if (allControlSchemesParameters[i].isAvailableForNewPlayer == false)
            {
                PlayerInputActions newPlayerInputActions = new PlayerInputActions();
                InputUser newInputUser = new InputUser();
                
                foreach(InputDevice connectedDevice in connectedDevices)
                {
                    if(allControlSchemesParameters[i].controlScheme.SupportsDevice(connectedDevice))
                    {
                        newInputUser = InputUser.PerformPairingWithDevice(connectedDevice);
                    }
                }

                if (newInputUser.pairedDevices.Count == 0)
                {
                    Debug.LogError("Trying to generate new PlayerInputActions but there is no relevant supported device connected");
                }

                newInputUser.AssociateActionsWithUser(newPlayerInputActions);
                newInputUser.ActivateControlScheme(allControlSchemesParameters[i].controlScheme.bindingGroup);
                newPlayerInputActions.Enable();

                numberOfPlayers++;

                allControlSchemesParameters[i].playerInputActions = newPlayerInputActions;

                GameInput.Instance.InitializePlayerInputActions(newPlayerInputActions);
            }
        }
    }

    public List<string> GetAvailableControlSchemesWithConnectedDevices()
    {
        availableControlSchemesWithConnectedDevices = new List<string>();
        foreach(InputDevice connectedDevice in connectedDevices)
        {
            for(int i = 0; i < allControlSchemesParameters.Length; i++)
            {
                if(allControlSchemesParameters[i].controlScheme.SupportsDevice(connectedDevice) && allControlSchemesParameters[i].isAvailableForNewPlayer)
                {
                    availableControlSchemesWithConnectedDevices.Add(allControlSchemesParameters[i].controlScheme.bindingGroup);
                }
            }
        }
        return availableControlSchemesWithConnectedDevices;
    }

    private string GetDeviceNameFromDeviceRequirement(InputControlScheme.DeviceRequirement deviceRequirement)
    {
        return deviceRequirement.ToString().Substring(deviceRequirement.ToString().IndexOf("<") + 1, deviceRequirement.ToString().IndexOf(">") - 1);
    }

    public List<string> GetSupportedDevicesNotConnected()
    {
        supportedDevicesNotConnected = new List<string>();
        //Add all supported devices to supportedDevicesNotConnected 
        foreach (ControlSchemeParameters schemeParameters in allControlSchemesParameters)
        {
            for (int j = 0; j < schemeParameters.controlScheme.deviceRequirements.Count; j++)
            {
                string deviceName = GetDeviceNameFromDeviceRequirement(schemeParameters.controlScheme.deviceRequirements[j]);
                if (!supportedDevicesNotConnected.Contains(deviceName))
                {
                    supportedDevicesNotConnected.Add(deviceName);
                }
            }
        }

        //Remove the connected devices which belong to supportedDevicesNotConnected
        foreach(InputDevice connectedDevice in connectedDevices)
        {
            if(supportedDevicesNotConnected.Contains(connectedDevice.name))
            {
                supportedDevicesNotConnected.Remove(connectedDevice.name);
            }

            // Unity uses different names for Gamepads in ControlSchemes.DeviceRequirements and InputSystem.devices.
            // The ugly solution I found was to split the Gamepad case appart.
            if(connectedDevice is Gamepad && supportedDevicesNotConnected.Contains("Gamepad"))
            {
                supportedDevicesNotConnected.Remove("Gamepad");
            }
        }
        return supportedDevicesNotConnected;
    } 

    public ControlSchemeParameters[] GetAllControlSchemeParameters()
    {
        return allControlSchemesParameters;
    }

    public bool IsNumberOfPlayersMaxReached()
    {
        return !(numberOfPlayers<numberOfPlayersMax);
    }

    public int GetMaxNumberOfPlayers()
    {
        return numberOfPlayersMax;
    }
}
