using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Linq;

public struct ControlSchemeParameters
    {
        public string bindingGroupName;
        public List<string> requiredDevices;
        public bool isAvailableForNewPlayer;
        public int playerID;
        public PlayerInputActions playerInputActions;

        public ControlSchemeParameters(string bindingGroupName, List<string> requiredDevices, bool isAvailableForNewPlayer, int playerID, PlayerInputActions playerInputActions)
        {
            this.bindingGroupName = bindingGroupName;
            this.requiredDevices = requiredDevices;
            this.isAvailableForNewPlayer = isAvailableForNewPlayer;
            this.playerID = playerID;
            this.playerInputActions = playerInputActions;
        }
    }

public class GameControlsManager : MonoBehaviour
{
    public static GameControlsManager Instance { get; private set; }

    [SerializeField] private int numberOfPlayersMax = 3;

    private static ControlSchemeParameters[] allControlSchemesParameters;
    private List<InputDevice> connectedDevices;
    private List<string> availableControlSchemesWithConnectedDevices;
    private List<string> supportedDevices;
    private List<string> supportedDevicesNotConnected;
    private List<string> selectedPlayerControls;
    private int numberOfPlayers;
    private PlayerInputActions defaultPlayerInputActions;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        LobbyUI.Instance.OnControlOptionLocked += LobbyUI_OnControlOptionSelected;
        LobbyUI.Instance.OnControlOptionUnlocked += LobbyUI_OnControlOptionUnselected;


        numberOfPlayers = 0;
        supportedDevices = new List<string>();
        connectedDevices = InputSystem.devices.ToList();
        selectedPlayerControls = new List<string>();

        defaultPlayerInputActions = new PlayerInputActions();

        CreateAllControlSchemesParameters();
    }

    private void LobbyUI_OnControlOptionSelected(object sender, LobbyUI.EventArgsOnControlOptionLocked e)
    {
        selectedPlayerControls.Add(e.selectedControlName);

        int indexSelectedControl = Array.FindIndex(allControlSchemesParameters, scheme => scheme.bindingGroupName == e.selectedControlName);
        allControlSchemesParameters[indexSelectedControl].isAvailableForNewPlayer = false;
    }

    private void LobbyUI_OnControlOptionUnselected(object sender, LobbyUI.EventArgsOnControlOptionUnlocked e)
    {
        selectedPlayerControls.Remove(e.unselectedControlName);

        int indexSelectedControl = Array.FindIndex(allControlSchemesParameters, scheme => scheme.bindingGroupName == e.unselectedControlName);
        allControlSchemesParameters[indexSelectedControl].isAvailableForNewPlayer = true;
    }


    private void Start()
    {
        //PlayerInputActions defaultPlayerInputActions = GameInput.Instance.GetDefaultPlayerInputActions();

        

        //InputSystem.onDeviceChange += InputSystem_OnDeviceChange;

    }

    private void CreateAllControlSchemesParameters()
    {
        allControlSchemesParameters = new ControlSchemeParameters[defaultPlayerInputActions.controlSchemes.Count];
        for (int i = 0; i < defaultPlayerInputActions.controlSchemes.Count; i++)
        {
            allControlSchemesParameters[i].bindingGroupName = defaultPlayerInputActions.controlSchemes[i].bindingGroup;
            allControlSchemesParameters[i].isAvailableForNewPlayer = true;
            allControlSchemesParameters[i].requiredDevices = new List<string>();

            for (int j = 0; j < defaultPlayerInputActions.controlSchemes[i].deviceRequirements.Count; j++)
            {
                allControlSchemesParameters[i].requiredDevices.Add(GetDeviceNameFromDeviceRequirement(defaultPlayerInputActions.controlSchemes[i].deviceRequirements[j]));

                //Store supported devices in the list supportedDevices
                string deviceName = GetDeviceNameFromDeviceRequirement(defaultPlayerInputActions.controlSchemes[i].deviceRequirements[j]);
                if(!supportedDevices.Contains(deviceName))
                {
                    supportedDevices.Add(deviceName);
                }
            }
        }
    }

    /*private void InputSystem_OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
                Debug.Log("Device added: " + device);
                break;
            case InputDeviceChange.Removed:
                Debug.Log("Device removed: " + device);
                break;
            case InputDeviceChange.ConfigurationChanged:
                Debug.Log("Device configuration changed: " + device);
                break;
        }
    }
    */
    public PlayerInputActions SetPlayerControlScheme(string selectedControl)
    {
        int indexControlsNextPlayer = Array.FindIndex(allControlSchemesParameters, scheme => scheme.bindingGroupName == selectedControl);
        allControlSchemesParameters[indexControlsNextPlayer].isAvailableForNewPlayer = false;

        string bindingGroupName = allControlSchemesParameters[indexControlsNextPlayer].bindingGroupName;
        List<string> devices = allControlSchemesParameters[indexControlsNextPlayer].requiredDevices;

        PlayerInputActions newPlayerInputActions = new PlayerInputActions();
        InputUser newInputUser = new InputUser();

        foreach(string supportedDevice in supportedDevices)
        {
            if(devices.Contains(supportedDevice))
            {
                InputDevice connectedDevice = connectedDevices.First(device => device.name == supportedDevice);
                newInputUser = InputUser.PerformPairingWithDevice(connectedDevice);
            }
        }

        if(newInputUser.pairedDevices.Count == 0)
        {
            Debug.LogError("Trying to generate new PlayerInputActions but there is no relevant supported device connected");
        }

        newInputUser.AssociateActionsWithUser(newPlayerInputActions);
        newInputUser.ActivateControlScheme(bindingGroupName);
        newPlayerInputActions.Enable();

        numberOfPlayers++;
        allControlSchemesParameters[indexControlsNextPlayer].playerID = numberOfPlayers;
        allControlSchemesParameters[indexControlsNextPlayer].playerInputActions = newPlayerInputActions;

        GameInput.Instance.InitializePlayerInputActions(newPlayerInputActions);

        return newPlayerInputActions;
    }

    public List<string> GetAvailableControlSchemesWithConnectedDevices()
    {
        availableControlSchemesWithConnectedDevices = new List<string>();
        foreach(InputDevice connectedDevice in connectedDevices)
        {
           foreach(ControlSchemeParameters schemeParameters in allControlSchemesParameters)
            {
                if(schemeParameters.requiredDevices.Contains(connectedDevice.name) && schemeParameters.isAvailableForNewPlayer)
                {
                    availableControlSchemesWithConnectedDevices.Add(schemeParameters.bindingGroupName);
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
        supportedDevicesNotConnected = new List<string>(supportedDevices);
        foreach(InputDevice device in connectedDevices)
        {
            if(supportedDevicesNotConnected.Contains(device.name))
            {
                supportedDevicesNotConnected.Remove(device.name);
            }
        }

        return supportedDevicesNotConnected;
    } 

    public ControlSchemeParameters[] GetAllControlSchemeParameters()
    {
        return allControlSchemesParameters;
    }

    public void SetNextPlayerControlScheme(string controlScheme)
    {
        selectedPlayerControls.Add(controlScheme);
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
