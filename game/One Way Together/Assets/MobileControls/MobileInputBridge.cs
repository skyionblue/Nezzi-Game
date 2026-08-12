using UnityEngine;
using OneWayTogether.Input;

/// <summary>
/// Bridges the Fenerax on-screen Joystick (default Assembly-CSharp) to the
/// game's InputRouter (OneWayTogether assembly). Lives in the default assembly
/// so it can reference both the third-party Joystick and the game's InputRouter.
///
/// Polls the joystick each frame; when pushed past the deadzone, forwards the
/// direction to InputRouter.SetMoveInput. Keyboard/gamepad input still flows
/// through InputRouter's own InputAction handlers independently.
///
/// The Interact and Switch on-screen buttons should wire their OnClick directly
/// to InputRouter.TriggerInteract and InputRouter.TrySwitchCharacter.
/// </summary>
public class MobileInputBridge : MonoBehaviour
{
    [SerializeField] private Joystick _joystick;
    [SerializeField] private InputRouter _inputRouter;

    [Tooltip("Below this magnitude the joystick is treated as released.")]
    [SerializeField, Range(0.05f, 0.5f)] private float _deadzone = 0.15f;

    private void Update()
    {
        if (_joystick == null || _inputRouter == null) return;

        Vector2 dir = _joystick.Direction;
        if (dir.sqrMagnitude > _deadzone * _deadzone)
            _inputRouter.SetMoveInput(dir);
    }
}
