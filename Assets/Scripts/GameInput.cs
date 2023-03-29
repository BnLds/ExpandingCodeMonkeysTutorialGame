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

    public struct controlSchemesAllocation
    {
        public int playerID;
        public PlayerInputActions playerInputActions;

        public controlSchemesAllocation(int playerID, PlayerInputActions playerInputActions)
        {
            this.playerID = playerID;
            this.playerInputActions = playerInputActions;
        }
    }

    private List<string> availableControlSchemes;
    private List<InputDevice> connectedDevices;
    private List<string> availableControlSchemesWithConnectedDevices;

    private int numberOfPlayers;
    private static controlSchemesAllocation[] controlSchemesAllocationArray;
    private int numberOfPlayersMax = 3;
    private PlayerInputActions defaultPlayerInputActions;
    private string nextPlayerControlScheme;

    private void Awake() 
    {
        Instance = this;

        numberOfPlayers = 0;
        controlSchemesAllocationArray = new controlSchemesAllocation[numberOfPlayersMax];
        
        connectedDevices = InputSystem.devices.ToList();
        availableControlSchemes = new List<string>() {KEYBOARD_WASD_SCHEME, KEYBOARD_ARROWS_SCHEME, GAMEPAD_SCHEME};

        defaultPlayerInputActions = new PlayerInputActions();

        if(PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            defaultPlayerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }
    }

    private void UpdateConnectedDevices()
    {
        foreach(InputDevice connectedDevice in InputSystem.devices.ToList())
        {
            if(!connectedDevices.Contains(connectedDevice))
            {
                connectedDevices.Add(connectedDevice);
            }
        }

        foreach(InputDevice formerConnectedDevices in connectedDevices)
        {
            if(!InputSystem.devices.ToList().Contains(formerConnectedDevices))
            {
                connectedDevices.Remove(formerConnectedDevices);
            }
        }
    }

    private void InitializePlayerInputActions(PlayerInputActions playerInputActions)
    {
        numberOfPlayers++;

        controlSchemesAllocationArray[numberOfPlayers-1] = new controlSchemesAllocation(numberOfPlayers, playerInputActions);
        
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

        foreach (controlSchemesAllocation allocation in controlSchemesAllocationArray)
        {
            if (allocation.playerInputActions != null)
            {
                LoadPlayerPrefs(allocation.playerInputActions);
            }
        }
    }

    public PlayerInputActions SetPlayerControlScheme()
    {
        availableControlSchemes.Remove(nextPlayerControlScheme);

        PlayerInputActions newPlayerInputActions = new PlayerInputActions();

        switch(nextPlayerControlScheme)
        {
            default:
                Debug.LogError("Unable to return PlayerInputActions for control scheme: "+ nextPlayerControlScheme);

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
                Debug.Log("returned PlayerInputActions for " + KEYBOARD_WASD_SCHEME);
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
            //on them. Instead of comairing devices types, I will compare their names.
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
        }
        foreach(var element in availableControlSchemesWithConnectedDevices)
        {
            Debug.Log(element);
        }
    }

    public List<string> GetAvailableControlSchemesWithConnectedDevices()
    {
        UpdateConnectedDevices();
        UpdateAvailableControlSchemesWithConnectedDevices();
        return availableControlSchemesWithConnectedDevices;
    }

    

    public void SetNextPlayerControlScheme(string controlScheme)
    {
        nextPlayerControlScheme = controlScheme;
    }

}
