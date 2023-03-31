using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Linq;

public struct ControlSchemesAllocation
{
    public int playerID;
    public PlayerInputActions playerInputActions;
    //public InputDevice inputDevice;

    public ControlSchemesAllocation(int playerID, PlayerInputActions playerInputActions)
    {
        this.playerID = playerID;
        this.playerInputActions = playerInputActions;
    }
}

public class GameControlsManager : MonoBehaviour
{
    public static GameControlsManager Instance { get; private set; }

    private const string KEYBOARD_WASD_SCHEME = "KeyboardWASD";
    private const string KEYBOARD_ARROWS_SCHEME = "KeyboardArrows";
    private const string GAMEPAD_SCHEME = "Gamepad";

    public struct ControlSchemeParameters
    {
        public string bindingGroupName;
        public List<string> requiredDevices;
        public bool isAvailableForNewPlayer;

        public ControlSchemeParameters(string bindingGroupName, List<string> requiredDevices, bool isAvailableForNewPlayer)
        {
            this.bindingGroupName = bindingGroupName;
            this.requiredDevices = requiredDevices;
            this.isAvailableForNewPlayer = isAvailableForNewPlayer;
        }
        
    }

    private static ControlSchemeParameters[] allControlSchemesParameters;
    private List<InputDevice> connectedDevices;
    private List<string> availableControlSchemesWithConnectedDevices;
    private List<string> supportedDevices;
    private List<string> supportedDevicesNotConnected;
    private static ControlSchemesAllocation[] controlSchemesAllocationArray;
    private string nextPlayerControlSchemeName;
    private int numberOfPlayers;
    private int numberOfPlayersMax = 3;

    private void Awake()
    {

        Instance = this;

        numberOfPlayers = 0;
        controlSchemesAllocationArray = new ControlSchemesAllocation[numberOfPlayersMax];
        supportedDevices = new List<string>();
        connectedDevices = InputSystem.devices.ToList();

    }

    private void Start()
    {
        PlayerInputActions defaultPlayerInputActions = GameInput.Instance.GetDefaultPlayerInputActions();
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
        
        //InputSystem.onDeviceChange += InputSystem_OnDeviceChange;

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
    public PlayerInputActions SetPlayerControlScheme()
    {
        int index = Array.FindIndex(allControlSchemesParameters, scheme => scheme.bindingGroupName == nextPlayerControlSchemeName);
        allControlSchemesParameters[index].isAvailableForNewPlayer = false;

        PlayerInputActions newPlayerInputActions = new PlayerInputActions();

        switch(nextPlayerControlSchemeName)
        {
            default:
                Debug.LogError("Unable to return PlayerInputActions for control scheme: "+ nextPlayerControlSchemeName);

                GameInput.Instance.InitializePlayerInputActions(newPlayerInputActions);
                return newPlayerInputActions;

            case(KEYBOARD_WASD_SCHEME):

                string WASD_SchemeName = "KeyboardWASD";

                InputUser newUserWASD = InputUser.PerformPairingWithDevice(Keyboard.current);
                InputUser.PerformPairingWithDevice(Mouse.current, newUserWASD);
                newUserWASD.AssociateActionsWithUser(newPlayerInputActions);
                newUserWASD.ActivateControlScheme(WASD_SchemeName);
                newPlayerInputActions.Enable();

                numberOfPlayers++;
                controlSchemesAllocationArray[numberOfPlayers-1] = new ControlSchemesAllocation(numberOfPlayers, newPlayerInputActions);
                GameInput.Instance.InitializePlayerInputActions(newPlayerInputActions);

                return newPlayerInputActions;

            case(KEYBOARD_ARROWS_SCHEME):

                string Arrows_SchemeName = "KeyboardArrows";

                InputUser newUserArrows = InputUser.PerformPairingWithDevice(Keyboard.current);
                InputUser.PerformPairingWithDevice(Mouse.current, newUserArrows);
                newUserArrows.AssociateActionsWithUser(newPlayerInputActions);
                newUserArrows.ActivateControlScheme(Arrows_SchemeName);
                newPlayerInputActions.Enable();

                numberOfPlayers++;
                controlSchemesAllocationArray[numberOfPlayers-1] = new ControlSchemesAllocation(numberOfPlayers, newPlayerInputActions);
                GameInput.Instance.InitializePlayerInputActions(newPlayerInputActions);

                return newPlayerInputActions;
            
            case(GAMEPAD_SCHEME):

                string Gamepad_SchemeName = "Gamepad";

                InputUser newUserGamepad = InputUser.PerformPairingWithDevice(Gamepad.all[0]);
                newUserGamepad.AssociateActionsWithUser(newPlayerInputActions);
                newUserGamepad.ActivateControlScheme(Gamepad_SchemeName);
                newPlayerInputActions.Enable();
                
                numberOfPlayers++;
                controlSchemesAllocationArray[numberOfPlayers-1] = new ControlSchemesAllocation(numberOfPlayers, newPlayerInputActions);
                GameInput.Instance.InitializePlayerInputActions(newPlayerInputActions);

                return newPlayerInputActions;
        }
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

    public ControlSchemesAllocation[] GetControlSchemesAllocationsArray()
    {
        return controlSchemesAllocationArray;
    }

    public void SetNextPlayerControlScheme(string controlScheme)
    {
        nextPlayerControlSchemeName = controlScheme;
    }

    public int GetNumberOfPlayers()
    {
        return numberOfPlayers;
    }


}
