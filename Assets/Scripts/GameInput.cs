using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Linq;


public class GameInput : MonoBehaviour
{

    private const string PLAYER_PREFS_BINDINGS = "InputBindings";
    private const string KEYBOARD_WASD_SCHEME = "Keyboard_WASD";
    private const string KEYBOARD_ARROWS_SCHEME = "Keyboard_Arrows";
    private const string GAMEPAD_SCHEME = "Gamepad";
    private const string KEYBOARD_NAME = "Keyboard";
    private const string GAMEPAD_NAME = "Gamepad";



    public static GameInput Instance {get; private set;}

    public event EventHandler<OnInteractActionEventArgs> OnInteractAction;
    public class OnInteractActionEventArgs: EventArgs
    {
        public InputAction.CallbackContext action;
    }

    public event EventHandler<OnInteractAlternateActionEventArgs> OnInteractAlternateAction;
    public class OnInteractAlternateActionEventArgs: EventArgs
    {
        public InputAction.CallbackContext action;
    }

    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingRebind;

    public enum Binding
    {
        Move_Up_WASD,
        Move_Down_WASD,
        Move_Left_WASD,
        Move_Right_WASD,
        Interact_WASD,
        Interact_Alt_WASD,
        Move_Up_Arrows,
        Move_Down_Arrows,
        Move_Left_Arrows,
        Move_Right_Arrows,
        Interact_Arrows,
        Interact_Alt_Arrows,
        Pause,
        Gamepad_Interact,
        Gamepad_Interact_Alternate,
        Gamepad_Pause

    }

    public enum ControlSchemes
    {
        Keyboard_WASD,
        Keyboard_Arrows,
        Gamepad
    }

    public struct ControlSchemesAllocation
    {
        public int playerID;
        public PlayerInputActions playerInputActions;
        //public InputDevice inputDevice;

        public ControlSchemesAllocation(int playerID, PlayerInputActions playerInputActions/*, InputDevice inputDevice*/)
        {
            this.playerID = playerID;
            this.playerInputActions = playerInputActions;
            //this.inputDevice = inputDevice;
        }
    }

    public struct ControlScheme_RequiredDevices
    {
        public string bindingGroupName;
        public List<string> requiredDevices;
        public bool isAvailableForUse;

        public ControlScheme_RequiredDevices(string bindingGroupName, List<string> requiredDevices, bool isAvailableForUse)
        {
            this.bindingGroupName = bindingGroupName;
            this.requiredDevices = requiredDevices;
            this.isAvailableForUse = isAvailableForUse;
        }
        
    }

    private List<string> availableControlSchemes;
    private static ControlScheme_RequiredDevices[] availableControlSchemes_RequiredDevices;
    private List<InputDevice> connectedDevices;
    private List<string> availableControlSchemesWithConnectedDevices;
    private List<string> supportedDevices;
    private List<string> supportedDevicesNotConnected;

    private int numberOfPlayers;
    private static ControlSchemesAllocation[] controlSchemesAllocationArray;
    private int numberOfPlayersMax = 3;
    private PlayerInputActions defaultPlayerInputActions;
    private string nextPlayerControlSchemeName;

    private void Awake() 
    {
        Instance = this;

        defaultPlayerInputActions = new PlayerInputActions();

        numberOfPlayers = 0;
        controlSchemesAllocationArray = new ControlSchemesAllocation[numberOfPlayersMax];

        supportedDevices = new List<string>();

        connectedDevices = InputSystem.devices.ToList();
        
        availableControlSchemes = new List<string>() {KEYBOARD_WASD_SCHEME, KEYBOARD_ARROWS_SCHEME, GAMEPAD_SCHEME};

        availableControlSchemes_RequiredDevices = new ControlScheme_RequiredDevices[defaultPlayerInputActions.controlSchemes.Count];
        foreach(InputControlScheme controlScheme in defaultPlayerInputActions.controlSchemes)
        {
            availableControlSchemes.Add(controlScheme.bindingGroup);
        }

        for (int i = 0; i < defaultPlayerInputActions.controlSchemes.Count; i++)
        {
            availableControlSchemes_RequiredDevices[i].bindingGroupName = defaultPlayerInputActions.controlSchemes[i].bindingGroup;
            availableControlSchemes_RequiredDevices[i].isAvailableForUse = true;
            availableControlSchemes_RequiredDevices[i].requiredDevices = new List<string>();

            for (int j = 0; j < defaultPlayerInputActions.controlSchemes[i].deviceRequirements.Count; j++)
            {
                availableControlSchemes_RequiredDevices[i].requiredDevices.Add(GetRequiredDeviceNameFromDeviceRequirement(defaultPlayerInputActions.controlSchemes[i].deviceRequirements[j]));

                //Store supported devices in the list supportedDevices
                string deviceName = GetRequiredDeviceNameFromDeviceRequirement(defaultPlayerInputActions.controlSchemes[i].deviceRequirements[j]);
                if(!supportedDevices.Contains(deviceName))
                {
                    supportedDevices.Add(deviceName);
                }
            }
        }

        //Load saved binding preferences
        if(PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            defaultPlayerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }
    }

    private void Start()
    {
        InputSystem.onDeviceChange += InputSystem_OnDeviceChange;

    }

    private void InputSystem_OnDeviceChange(InputDevice device, InputDeviceChange change)
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

    private void InitializePlayerInputActions(PlayerInputActions playerInputActions)
    {
        numberOfPlayers++;

        controlSchemesAllocationArray[numberOfPlayers-1] = new ControlSchemesAllocation(numberOfPlayers, playerInputActions);
        
        LoadPlayerPrefs(playerInputActions);
        SubscribeToInputActions(playerInputActions);
    }

    private void LoadPlayerPrefs (PlayerInputActions playerInputActions)
    {
        if(PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }
    }

    private void SubscribeToInputActions(PlayerInputActions playerInputActions)
    {
        playerInputActions.Player.Enable();      

        playerInputActions.Player.Interact.performed += Interact_Performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_Performed;
        playerInputActions.Player.Pause.performed += Pause_Performed;
    } 

    public void DestroyPlayerInputActions(PlayerInputActions playerInputActions)
    {
        UnsubscribeToInputActions(playerInputActions);
    }

    private void UnsubscribeToInputActions(PlayerInputActions playerInputActions)
    {
        playerInputActions.Player.Interact.performed -= Interact_Performed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_Performed;
        playerInputActions.Player.Pause.performed -= Pause_Performed;

        playerInputActions.Dispose();
    }

    private void Pause_Performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_Performed(InputAction.CallbackContext action)
    {
        OnInteractAlternateAction?.Invoke(this, new OnInteractAlternateActionEventArgs
        {
            action = action
        });
    }

    private void Interact_Performed(UnityEngine.InputSystem.InputAction.CallbackContext action)
    {
        OnInteractAction?.Invoke(this, new OnInteractActionEventArgs
        {
            action = action
        });
    }

    public Vector2 GetMovementVectorNormalized(PlayerInputActions playerInputActions)
    {
        Vector2 inputVector2 = playerInputActions.Player.Move.ReadValue<Vector2>();
        inputVector2 = inputVector2.normalized;

        return inputVector2;
    }

    public PlayerInputActions SetPlayerControlScheme()
    {
        availableControlSchemes.Remove(nextPlayerControlSchemeName);
        /*ControlScheme_RequiredDevices nextPlayerControlScheme = availableControlSchemes_RequiredDevices.First(scheme => scheme.bindingGroupName == nextPlayerControlSchemeName);
        Debug.Log(nextPlayerControlScheme.bindingGroupName);
        Debug.Log(nextPlayerControlScheme.requiredDevices);
        Debug.Log(nextPlayerControlScheme.isAvailableForUse);
        */

        PlayerInputActions newPlayerInputActions = new PlayerInputActions();

        switch(nextPlayerControlSchemeName)
        {
            default:
                Debug.LogError("Unable to return PlayerInputActions for control scheme: "+ nextPlayerControlSchemeName);

                InitializePlayerInputActions(newPlayerInputActions);
                return newPlayerInputActions;

            case(KEYBOARD_WASD_SCHEME):

                string WASD_SchemeName = "KeyboardWASD";

                InputUser newUserWASD = InputUser.PerformPairingWithDevice(Keyboard.current);
                InputUser.PerformPairingWithDevice(Mouse.current, newUserWASD);
                newUserWASD.AssociateActionsWithUser(newPlayerInputActions);
                newUserWASD.ActivateControlScheme(WASD_SchemeName);
                newPlayerInputActions.Enable();

                InitializePlayerInputActions(newPlayerInputActions);

                return newPlayerInputActions;

            case(KEYBOARD_ARROWS_SCHEME):

                string Arrows_SchemeName = "KeyboardArrows";

                InputUser newUserArrows = InputUser.PerformPairingWithDevice(Keyboard.current);
                InputUser.PerformPairingWithDevice(Mouse.current, newUserArrows);
                newUserArrows.AssociateActionsWithUser(newPlayerInputActions);
                newUserArrows.ActivateControlScheme(Arrows_SchemeName);
                newPlayerInputActions.Enable();

                InitializePlayerInputActions(newPlayerInputActions);

                return newPlayerInputActions;
            
            case(GAMEPAD_SCHEME):

                string Gamepad_SchemeName = "Gamepad";

                InputUser newUserGamepad = InputUser.PerformPairingWithDevice(Gamepad.all[0]);
                newUserGamepad.AssociateActionsWithUser(newPlayerInputActions);
                newUserGamepad.ActivateControlScheme(Gamepad_SchemeName);
                newPlayerInputActions.Enable();

                InitializePlayerInputActions(newPlayerInputActions);

                return newPlayerInputActions;
        }
    }

    public int GetNumberOfPlayers()
    {
        return numberOfPlayers;
    }

    public bool isActionMine(InputAction.CallbackContext obj, PlayerInputActions playerInputActions)
    {    
        return (playerInputActions.Contains(obj.action));
    }
 
    public PlayerInputActions GetDefaultPlayerInputActions()
    {
        return defaultPlayerInputActions;
    }

    private void UpdateAvailableControlSchemesWithConnectedDevices()
    {
        availableControlSchemesWithConnectedDevices = new List<string>();
        foreach(InputDevice connectedDevice in connectedDevices)
        {
            //For some reason, Unity identifies my Keyboard and Mouse types as FastKeyboard and FastMouse. 
            //However these types can’t be accessed through code and there doesn’t seem to be any documentation 
            //on them. Instead of comparing devices types, I will compare their names.
            if(connectedDevice.name == KEYBOARD_NAME && (availableControlSchemes.Contains(KEYBOARD_ARROWS_SCHEME) || availableControlSchemes.Contains(KEYBOARD_WASD_SCHEME)))
            {
                if(availableControlSchemes.Contains(KEYBOARD_ARROWS_SCHEME))
                {
                    availableControlSchemesWithConnectedDevices.Add(KEYBOARD_ARROWS_SCHEME);
                }

                if(availableControlSchemes.Contains(KEYBOARD_WASD_SCHEME))
                {
                    availableControlSchemesWithConnectedDevices.Add(KEYBOARD_WASD_SCHEME);
                }
            }
            else if (connectedDevice.name == GAMEPAD_NAME && availableControlSchemes.Contains(GAMEPAD_SCHEME))
            {
                availableControlSchemesWithConnectedDevices.Add(GAMEPAD_SCHEME);
            }

        //ControlScheme_RequiredDevices nextPlayerControlScheme = availableControlSchemes_RequiredDevices.First(scheme => scheme.bindingGroupName == nextPlayerControlSchemeName);
           /* foreach(ControlScheme_RequiredDevices controlScheme_RequiredDevices in availableControlSchemes_RequiredDevices)
            {
                if(availableControlSchemes_RequiredDevices.Contains(scheme => scheme.requiredDevices. ))
            }*/


        }
    }

    private string GetRequiredDeviceNameFromDeviceRequirement(InputControlScheme.DeviceRequirement deviceRequirement)
    {
        return deviceRequirement.ToString().Substring(deviceRequirement.ToString().IndexOf("<") + 1, deviceRequirement.ToString().IndexOf(">") - 1);
    }

    public List<string> GetAvailableControlSchemesWithConnectedDevices()
    {
        UpdateAvailableControlSchemesWithConnectedDevices();
        return availableControlSchemesWithConnectedDevices;
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

    public void SetNextPlayerControlScheme(string controlScheme)
    {
        nextPlayerControlSchemeName = controlScheme;
    }

#region Binding
    public string GetBindingText(Binding binding)
    {
        switch(binding)
        {
            default:
            case Binding.Interact_WASD:
                return (defaultPlayerInputActions.Player.Interact.bindings[0].ToDisplayString());
            case Binding.Interact_Alt_WASD:
                return (defaultPlayerInputActions.Player.InteractAlternate.bindings[0].ToDisplayString());
            case Binding.Pause:
                return (defaultPlayerInputActions.Player.Pause.bindings[0].ToDisplayString());
            case Binding.Move_Up_WASD:
                return (defaultPlayerInputActions.Player.Move.bindings[1].ToDisplayString());
            case Binding.Move_Down_WASD:
                return (defaultPlayerInputActions.Player.Move.bindings[2].ToDisplayString());
            case Binding.Move_Left_WASD:
                return (defaultPlayerInputActions.Player.Move.bindings[3].ToDisplayString());
            case Binding.Move_Right_WASD:
                return (defaultPlayerInputActions.Player.Move.bindings[4].ToDisplayString());

            case Binding.Interact_Arrows:
                return (defaultPlayerInputActions.Player.Interact.bindings[1].ToDisplayString());
            case Binding.Interact_Alt_Arrows:
                return (defaultPlayerInputActions.Player.InteractAlternate.bindings[1].ToDisplayString());
            case Binding.Move_Up_Arrows:
                return (defaultPlayerInputActions.Player.Move.bindings[6].ToDisplayString());
            case Binding.Move_Down_Arrows:
                return defaultPlayerInputActions.Player.Move.bindings[7].ToDisplayString();
            case Binding.Move_Left_Arrows:
                return defaultPlayerInputActions.Player.Move.bindings[8].ToDisplayString();
            case Binding.Move_Right_Arrows:
                return defaultPlayerInputActions.Player.Move.bindings[9].ToDisplayString();

            case Binding.Gamepad_Interact:
                return defaultPlayerInputActions.Player.Interact.bindings[2].ToDisplayString();
            case Binding.Gamepad_Interact_Alternate:
                return defaultPlayerInputActions.Player.InteractAlternate.bindings[2].ToDisplayString();
            case Binding.Gamepad_Pause:
                return defaultPlayerInputActions.Player.Pause.bindings[1].ToDisplayString();
        }
    }

    public void RebindBinding(Binding binding, Action onActionRebound)
    {
        defaultPlayerInputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex;
        switch (binding)
        {
            default:
            case Binding.Move_Up_WASD:
                inputAction = defaultPlayerInputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.Move_Down_WASD:
                inputAction = defaultPlayerInputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.Move_Left_WASD:
                inputAction = defaultPlayerInputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Binding.Move_Right_WASD:
                inputAction = defaultPlayerInputActions.Player.Move;
                bindingIndex = 4;
                break;
            case Binding.Interact_WASD:
                inputAction = defaultPlayerInputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.Interact_Alt_WASD:
                inputAction = defaultPlayerInputActions.Player.InteractAlternate;
                bindingIndex = 0;
                break;
            case Binding.Pause:
                inputAction = defaultPlayerInputActions.Player.Pause;
                bindingIndex = 0;
                break;
            case Binding.Move_Up_Arrows:
                inputAction = defaultPlayerInputActions.Player.Move;
                bindingIndex = 6;
                break;
            case Binding.Move_Down_Arrows:
                inputAction = defaultPlayerInputActions.Player.Move;
                bindingIndex = 7;
                break;
            case Binding.Move_Left_Arrows:
                inputAction = defaultPlayerInputActions.Player.Move;
                bindingIndex = 8;
                break;
            case Binding.Move_Right_Arrows:
                inputAction = defaultPlayerInputActions.Player.Move;
                bindingIndex = 9;
                break;
            case Binding.Interact_Arrows:
                inputAction = defaultPlayerInputActions.Player.Interact;
                bindingIndex = 1;
                break;
            case Binding.Interact_Alt_Arrows:
                inputAction = defaultPlayerInputActions.Player.InteractAlternate;
                bindingIndex = 1;
                break;
            case Binding.Gamepad_Interact:
                inputAction = defaultPlayerInputActions.Player.Interact;
                bindingIndex = 2;
                break;
            case Binding.Gamepad_Interact_Alternate:
                inputAction = defaultPlayerInputActions.Player.InteractAlternate;
                bindingIndex = 2;
                break;
            case Binding.Gamepad_Pause:
                inputAction = defaultPlayerInputActions.Player.Pause;
                bindingIndex = 1;
                break;
        }
        inputAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                defaultPlayerInputActions.Player.Enable();

                onActionRebound();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, defaultPlayerInputActions.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();

                ReloadBindingsOnRebind();

                OnBindingRebind?.Invoke(this, EventArgs.Empty);
                
            })
            .Start();
    }
        
    private void ReloadBindingsOnRebind()
    {
        LoadPlayerPrefs(defaultPlayerInputActions);

        foreach (ControlSchemesAllocation allocation in controlSchemesAllocationArray)
        {
            if (allocation.playerInputActions != null)
            {
                LoadPlayerPrefs(allocation.playerInputActions);
            }
        }
    }

#endregion

}
