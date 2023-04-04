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
        public Material playerVisualMaterial;


    public ControlSchemeParameters(string bindingGroupName, List<string> requiredDevices, bool isAvailableForNewPlayer, int playerID, PlayerInputActions playerInputActions, Material playerVisualMaterial)
        {
            this.bindingGroupName = bindingGroupName;
            this.requiredDevices = requiredDevices;
            this.isAvailableForNewPlayer = isAvailableForNewPlayer;
            this.playerID = playerID;
            this.playerInputActions = playerInputActions;
            this.playerVisualMaterial = playerVisualMaterial;
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
    private int numberOfPlayers;
    private PlayerInputActions defaultPlayerInputActions;

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
        supportedDevices = new List<string>();
        connectedDevices = InputSystem.devices.ToList();

        defaultPlayerInputActions = new PlayerInputActions();

        CreateAllControlSchemesParameters();
    }

    private void LobbyUI_OnCharacterParametersSelected(object sender, LobbyUI.EventArgsOnCharacterParametersSelected e)
    {
        int indexSelectedControl = Array.FindIndex(allControlSchemesParameters, scheme => scheme.bindingGroupName == e.selectedControlName);
        allControlSchemesParameters[indexSelectedControl].isAvailableForNewPlayer = false;
        allControlSchemesParameters[indexSelectedControl].playerVisualMaterial = e.selectedSkinMaterial;
    }

    private void LobbyUI_OnCharacterParametersUnselected(object sender, LobbyUI.EventArgsOnCharacterParametersUnselected e)
    {
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
            allControlSchemesParameters[i].playerInputActions = null;

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
    public void GeneratePlayerInputActions(/*string selectedControl*/)
    {
        for (int i = 0; i < allControlSchemesParameters.Length; i++)
        {
            if (allControlSchemesParameters[i].isAvailableForNewPlayer == false)
            {
                string bindingGroupName = allControlSchemesParameters[i].bindingGroupName;
                List<string> requiredDevices = allControlSchemesParameters[i].requiredDevices;

                PlayerInputActions newPlayerInputActions = new PlayerInputActions();
                InputUser newInputUser = new InputUser();

                foreach (string supportedDevice in supportedDevices)
                {
                    if (requiredDevices.Contains(supportedDevice))
                    {
                        InputDevice connectedDevice;
                        // Unity uses different names for Gamepads in ControlSchemes.DeviceRequirements and InputSystem.devices.
                        // The ugly solution I found was to split the Gamepad case appart.
                        if (supportedDevice != "Gamepad")
                        {
                            connectedDevice = connectedDevices.First(device => device.name == supportedDevice);
                        }
                        else
                        {
                            connectedDevice = connectedDevices.First(device => device is Gamepad);
                        }
                        newInputUser = InputUser.PerformPairingWithDevice(connectedDevice);
                    }

                }

                if (newInputUser.pairedDevices.Count == 0)
                {
                    Debug.LogError("Trying to generate new PlayerInputActions but there is no relevant supported device connected");
                }

                newInputUser.AssociateActionsWithUser(newPlayerInputActions);
                newInputUser.ActivateControlScheme(bindingGroupName);
                newPlayerInputActions.Enable();

                numberOfPlayers++;

                allControlSchemesParameters[i].playerID = numberOfPlayers;
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
           foreach(ControlSchemeParameters schemeParameters in allControlSchemesParameters)
            {
                if(schemeParameters.requiredDevices.Contains(connectedDevice.name) && schemeParameters.isAvailableForNewPlayer)
                {
                    availableControlSchemesWithConnectedDevices.Add(schemeParameters.bindingGroupName);
                }

                // Again I made a special case of Gamepad, ugly solution but easy one
                if(connectedDevice is Gamepad && schemeParameters.requiredDevices.Contains("Gamepad") && schemeParameters.isAvailableForNewPlayer)
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

            // Again I made a special case of Gamepad, ugly solution but easy one
            if(device is Gamepad && supportedDevicesNotConnected.Contains("Gamepad"))
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
