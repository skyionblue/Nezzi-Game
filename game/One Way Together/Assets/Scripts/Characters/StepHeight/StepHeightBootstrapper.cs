using System;
using System.Reflection;
using LB.Player.Movement.StepHeight;
using UnityEngine;

namespace OneWayTogether.Characters.StepHeight
{
    /// <summary>
    /// Injects CharacterController-based adapters into StepHeightController after that
    /// component's Awake() has already run.
    ///
    /// WHY REFLECTION:
    /// StepHeightController.Awake() hard-codes construction of RigidbodyWrapper,
    /// ColliderManager, and MovementMovementInputManager — there is no virtual or
    /// inspector-exposed injection seam. Because Awake() is private it cannot be
    /// overridden. Reflection is the only mechanism that replaces those fields without
    /// modifying the package source. All field names are string constants so a rename
    /// in the package will produce a compile-time warning (caught in Start's null checks)
    /// rather than a silent failure.
    ///
    /// EXECUTION ORDER:
    /// Unity calls Awake() on all objects before any Start(). By injecting in Start()
    /// we are guaranteed that StepHeightController.Awake() has already populated its
    /// private fields, and we can safely replace them. We also re-subscribe the
    /// OnEnable input events because StepHeightController.OnEnable() ran against the
    /// original (wrong) IMovementInputManager instance.
    ///
    /// Attach this component to each character alongside StepHeightController,
    /// CCRigidbodyWrapper, CCMovementInputManager, and CCColliderManager.
    /// </summary>
    [RequireComponent(typeof(StepHeightController))]
    [RequireComponent(typeof(CCRigidbodyWrapper))]
    [RequireComponent(typeof(CCMovementInputManager))]
    [RequireComponent(typeof(CCColliderManager))]
    public sealed class StepHeightBootstrapper : MonoBehaviour
    {
        // Field names in StepHeightController — used for reflection lookup.
        private const string FieldRigidbodyWrapper    = "rigidbodyWrapper";
        private const string FieldMovementInputManager = "movementInputManager";
        private const string FieldColliderManager      = "colliderManager";

        private StepHeightController  _stepHeight;
        private CCRigidbodyWrapper    _rbWrapper;
        private CCMovementInputManager _inputManager;
        private CCColliderManager     _colliderManager;

        private void Awake()
        {
            _stepHeight      = GetComponent<StepHeightController>();
            _rbWrapper       = GetComponent<CCRigidbodyWrapper>();
            _inputManager    = GetComponent<CCMovementInputManager>();
            _colliderManager = GetComponent<CCColliderManager>();
        }

        private void Start()
        {
            // By Start(), all Awake() calls have completed — safe to overwrite.
            InjectAdapters();
        }

        // ── Injection ─────────────────────────────────────────────────────────────

        private void InjectAdapters()
        {
            Type shcType = typeof(StepHeightController);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

            bool success = true;
            success &= SetPrivateField(shcType, flags, FieldRigidbodyWrapper,    _rbWrapper);
            success &= SetPrivateField(shcType, flags, FieldMovementInputManager, _inputManager);
            success &= SetPrivateField(shcType, flags, FieldColliderManager,      _colliderManager);

            if (!success)
            {
                Debug.LogError(
                    $"[StepHeightBootstrapper] One or more private fields could not be found on " +
                    $"StepHeightController. The package may have been updated. " +
                    $"Check that field names '{FieldRigidbodyWrapper}', " +
                    $"'{FieldMovementInputManager}', and '{FieldColliderManager}' still exist.",
                    this);
                return;
            }

            // StepHeightController.OnEnable() subscribed to the old MovementMovementInputManager
            // before we injected. Disable+Enable forces re-subscription against our adapter.
            _stepHeight.enabled = false;
            _stepHeight.enabled = true;

            // Cache the correct collider measurements now that our CC-based manager is wired.
            _stepHeight.UpdateCachedPlayerColliderInfo();

            Debug.Log($"[StepHeightBootstrapper] Adapters injected successfully on '{gameObject.name}'.", this);
        }

        private bool SetPrivateField(Type type, BindingFlags flags, string fieldName, object value)
        {
            FieldInfo field = type.GetField(fieldName, flags);
            if (field == null)
            {
                Debug.LogError(
                    $"[StepHeightBootstrapper] Field '{fieldName}' not found on {type.Name}.", this);
                return false;
            }

            field.SetValue(_stepHeight, value);
            return true;
        }
    }
}
