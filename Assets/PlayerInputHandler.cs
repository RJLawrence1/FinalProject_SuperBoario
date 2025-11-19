using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public bool JumpPressed { get; private set; }
    public bool ChargeHeld { get; private set; }
    public bool PickupOrThrowPressed { get; private set; }

    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();

        // Jump
        controls.Gameplay.Jump.performed += _ => JumpPressed = true;

        // Charge
        controls.Gameplay.Charge.started += _ => ChargeHeld = true;
        controls.Gameplay.Charge.canceled += _ => ChargeHeld = false;

        // Pickup/Throw
        controls.Gameplay.PickupOrThrow.performed += _ => PickupOrThrowPressed = true;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void LateUpdate()
    {
        JumpPressed = false;
        PickupOrThrowPressed = false;
    }
}