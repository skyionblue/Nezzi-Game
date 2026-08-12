using LB.Player.Movement.StepHeight;
using UnityEngine;

namespace OneWayTogether.Characters.StepHeight
{
    /// <summary>
    /// Adapts CharacterController position and velocity to the IRigidbodyWrapper interface
    /// expected by StepHeightController.
    ///
    /// MovePosition disables the CharacterController, teleports via transform.position,
    /// then re-enables — the same pattern used by CheckpointManager when resetting
    /// character positions. This is necessary because CharacterController ignores direct
    /// transform mutations while it is active.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class CCRigidbodyWrapper : MonoBehaviour, IRigidbodyWrapper
    {
        private CharacterController _cc;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        // IRigidbodyWrapper ────────────────────────────────────────────────────────

        /// <summary>World-space velocity reported by the CharacterController.</summary>
        public Vector3 Velocity => _cc.velocity;

        /// <summary>Current world-space position of this character.</summary>
        public Vector3 Position => transform.position;

        /// <summary>
        /// Teleports the character to <paramref name="position"/> by disabling
        /// CharacterController, setting transform.position directly, then re-enabling.
        /// Called by StepHeightController's lerp coroutine each frame during a step.
        /// </summary>
        public void MovePosition(Vector3 position)
        {
            _cc.enabled = false;
            transform.position = position;
            _cc.enabled = true;
        }
    }
}
