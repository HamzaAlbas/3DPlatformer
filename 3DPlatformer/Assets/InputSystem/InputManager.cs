using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private PlayerInputActions actions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        actions = new PlayerInputActions();
    }

    private void OnEnable() => actions.Enable();

    private void OnDisable() => actions.Disable();

    public Vector2 GetMovement()
    {
        return actions.Gameplay.Move.ReadValue<Vector2>();
    }

    public bool GetJump()
    {
        return actions.Gameplay.Jump.triggered;
    }

    public bool GetDash()
    {
        return actions.Gameplay.Dash.triggered;
    }

    public bool GetGroundPound()
    {
        return actions.Gameplay.GroundPound.triggered;
    }
}
