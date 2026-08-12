using System;
using LB.Player.Movement.StepHeight;
using UnityEngine;

namespace OneWayTogether.Characters.StepHeight
{
    /// <summary>
    /// Bridges CharacterBase move input to the IMovementInputManager interface consumed
    /// by StepHeightController.
    ///
    /// CharacterBase calls <see cref="NotifyMove"/> and <see cref="NotifyStop"/> whenever
    /// its _moveInput changes. StepHeightController's coroutine checks hasMovementInput
    /// (driven by these events) to decide whether to abort an in-progress step.
    ///
    /// EnableMovementInput / DisableMovementInput are no-ops here because input routing
    /// is owned by InputRouter and CharacterBase — we never gate the events ourselves.
    /// </summary>
    public sealed class CCMovementInputManager : MonoBehaviour, IMovementInputManager
    {
        // IMovementInputManager ───────────────────────────────────────────────────

        public event Action<Vector2> OnMovementPerformed;
        public event Action OnMovementCanceled;

        /// <summary>No-op: input lifecycle is managed by InputRouter.</summary>
        public void EnableMovementInput() { }

        /// <summary>No-op: input lifecycle is managed by InputRouter.</summary>
        public void DisableMovementInput() { }

        // CharacterBase call-outs ────────────────────────────────────────────────

        /// <summary>
        /// Called by CharacterBase.ReceiveMove — forwards live input to any listeners
        /// (StepHeightController uses this to set its internal hasMovementInput flag).
        /// </summary>
        public void NotifyMove(Vector2 input) => OnMovementPerformed?.Invoke(input);

        /// <summary>
        /// Called by CharacterBase.ReceiveStopMove — signals that movement has ended
        /// so StepHeightController can abort a step-up coroutine if needed.
        /// </summary>
        public void NotifyStop() => OnMovementCanceled?.Invoke();
    }
}
