using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


public class GameInput : MonoBehaviour
{

    private const string PLAYER_PREFS_BINDINGS = "InputBindings";

    public static GameInput Instance {get; private set;}

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
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

    private PlayerInputActions playerInputActions;

    private void Awake() 
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();

        if(PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }

        playerInputActions.Player.Enable();      

        playerInputActions.Player.Interact.performed += Interact_Performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_Performed;
        playerInputActions.Player.Pause.performed += Pause_Performed;

    }

    

    private void OnDestroy()
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

    private void InteractAlternate_Performed(InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_Performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }
    public Vector2 GetMovementVectorNormalized()
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
                return (playerInputActions.Player.Interact.bindings[0].ToDisplayString());
            case Binding.Interact_Alt_WASD:
                return (playerInputActions.Player.InteractAlternate.bindings[0].ToDisplayString());
            case Binding.Pause:
                return (playerInputActions.Player.Pause.bindings[0].ToDisplayString());
            case Binding.Move_Up_WASD:
                return (playerInputActions.Player.Move.bindings[1].ToDisplayString());
            case Binding.Move_Down_WASD:
                return (playerInputActions.Player.Move.bindings[2].ToDisplayString());
            case Binding.Move_Left_WASD:
                return (playerInputActions.Player.Move.bindings[3].ToDisplayString());
            case Binding.Move_Right_WASD:
                return (playerInputActions.Player.Move.bindings[4].ToDisplayString());

            case Binding.Interact_Arrows:
                return (playerInputActions.Player.Interact.bindings[1].ToDisplayString());
            case Binding.Interact_Alt_Arrows:
                return (playerInputActions.Player.InteractAlternate.bindings[1].ToDisplayString());
            case Binding.Move_Up_Arrows:
                return (playerInputActions.Player.Move.bindings[6].ToDisplayString());
            case Binding.Move_Down_Arrows:
                return playerInputActions.Player.Move.bindings[7].ToDisplayString();
            case Binding.Move_Left_Arrows:
                return playerInputActions.Player.Move.bindings[8].ToDisplayString();
            case Binding.Move_Right_Arrows:
                return playerInputActions.Player.Move.bindings[9].ToDisplayString();

            case Binding.Gamepad_Interact:
                return playerInputActions.Player.Interact.bindings[2].ToDisplayString();
            case Binding.Gamepad_Interact_Alternate:
                return playerInputActions.Player.InteractAlternate.bindings[2].ToDisplayString();
            case Binding.Gamepad_Pause:
                return playerInputActions.Player.Pause.bindings[1].ToDisplayString();
        }
    }

    public void RebindBinding(Binding binding, Action onActionRebound)
    {
        playerInputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex;

        switch(binding) {
            default:
            case Binding.Move_Up_WASD:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.Move_Down_WASD:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.Move_Left_WASD:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Binding.Move_Right_WASD:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 4;
                break;
            case Binding.Interact_WASD:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.Interact_Alt_WASD:
                inputAction = playerInputActions.Player.InteractAlternate;
                bindingIndex = 0;
                break;
            case Binding.Pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 0;
                break;

            case Binding.Move_Up_Arrows:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 6;
                break;
            case Binding.Move_Down_Arrows:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 7;
                break;
            case Binding.Move_Left_Arrows:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 8;
                break;
            case Binding.Move_Right_Arrows:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 9;
                break;
            case Binding.Interact_Arrows:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 1;
                break;
            case Binding.Interact_Alt_Arrows:
                inputAction = playerInputActions.Player.InteractAlternate;
                bindingIndex = 1;
                break;

            case Binding.Gamepad_Interact:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 2;
                break;
            case Binding.Gamepad_Interact_Alternate:
                inputAction = playerInputActions.Player.InteractAlternate;
                bindingIndex = 2;
                break;
            case Binding.Gamepad_Pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 1;
                break;
        }

        inputAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete( callback => 
            {
                callback.Dispose();
                playerInputActions.Player.Enable();
                onActionRebound();    

                
                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInputActions.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();

                OnBindingRebind?.Invoke(this, EventArgs.Empty);
            }) 
            .Start();

    }

    public PlayerInputActions SetPlayerControlScheme(ControlSchemes controlScheme)
    {
        PlayerInputActions newPlayerInputActions = new PlayerInputActions();

        switch(controlScheme)
        {
            default:
            case(ControlSchemes.Keyboard_WASD):

                string WASD_SchemeName = "KeyboardWASD";

                InputUser newUserWASD = InputUser.PerformPairingWithDevice(Keyboard.current);
                InputUser.PerformPairingWithDevice(Mouse.current, newUserWASD);
                newUserWASD.AssociateActionsWithUser(newPlayerInputActions);
                newUserWASD.ActivateControlScheme(WASD_SchemeName);
                newPlayerInputActions.Enable();

                return newPlayerInputActions;

            case(ControlSchemes.Keyboard_Arrows):

                string Arrows_SchemeName = "KeyboardArrows";

                InputUser newUserArrows = InputUser.PerformPairingWithDevice(Keyboard.current);
                InputUser.PerformPairingWithDevice(Mouse.current, newUserArrows);
                newUserArrows.AssociateActionsWithUser(newPlayerInputActions);
                newUserArrows.ActivateControlScheme(Arrows_SchemeName);
                newPlayerInputActions.Enable();

                return newPlayerInputActions;
            
            case(ControlSchemes.Gamepad):

                string Gamepad_SchemeName = "Gamepad";

                InputUser newUserGamepad = InputUser.PerformPairingWithDevice(Gamepad.all[0]);
                newUserGamepad.AssociateActionsWithUser(newPlayerInputActions);
                newUserGamepad.ActivateControlScheme(Gamepad_SchemeName);
                newPlayerInputActions.Enable();

                return newPlayerInputActions;
        }
        
        
    }
    
}
