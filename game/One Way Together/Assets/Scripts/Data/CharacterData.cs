using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Data
{
    /// <summary>
    /// ScriptableObject that holds all tunable configuration for a single character.
    /// Create one asset for Scarlet and one for Dani via
    /// Assets > Create > OneWayTogether > Character Data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterData", menuName = "OneWayTogether/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Which character this data describes.")]
        [SerializeField] private CharacterType _characterType;

        [Tooltip("Display name shown in the UI.")]
        [SerializeField] private string _displayName;

        [Header("Movement")]
        [Tooltip("Horizontal movement speed in units per second.")]
        [SerializeField, Range(1f, 20f)] private float _moveSpeed = 5f;

        [Tooltip("Jump force applied as an instantaneous velocity.")]
        [SerializeField, Range(1f, 30f)] private float _jumpForce = 10f;

        [Tooltip("Extra downward gravity multiplier applied when the character is falling.")]
        [SerializeField, Range(1f, 5f)] private float _fallMultiplier = 2.5f;

        [Tooltip("Gravity multiplier applied when the jump button is released early (short hop).")]
        [SerializeField, Range(1f, 5f)] private float _lowJumpMultiplier = 2f;

        [Header("Ground Detection")]
        [Tooltip("Radius of the OverlapCircle used to detect ground contact.")]
        [SerializeField, Range(0.05f, 0.5f)] private float _groundCheckRadius = 0.15f;

        [Tooltip("Layer mask that counts as ground.")]
        [SerializeField] private LayerMask _groundLayer;

        [Header("Animation")]
        [Tooltip("Animator Controller to use for this character.")]
        [SerializeField] private RuntimeAnimatorController _animatorController;

        // ── Public accessors ─────────────────────────────────────────────────────

        public CharacterType CharacterType => _characterType;
        public string DisplayName => _displayName;
        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public float FallMultiplier => _fallMultiplier;
        public float LowJumpMultiplier => _lowJumpMultiplier;
        public float GroundCheckRadius => _groundCheckRadius;
        public LayerMask GroundLayer => _groundLayer;
        public RuntimeAnimatorController AnimatorController => _animatorController;
    }
}
