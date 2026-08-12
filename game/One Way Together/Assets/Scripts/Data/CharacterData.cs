using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Data
{
    /// <summary>
    /// ScriptableObject that holds all tunable configuration for a single character.
    /// Create one asset for Scarlet and one for Dani via
    /// Assets > Create > OneWayTogether > Character Data.
    ///
    /// Jump, fall multiplier, ground check, and ground layer fields have been removed —
    /// the game uses top-down free XY movement with no gravity.
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
        [Tooltip("Walk speed — used when the joystick is pushed gently (magnitude < 0.5).")]
        [SerializeField, Range(1f, 20f)] private float _moveSpeed = 5f;

        [Tooltip("Run speed — used when the joystick is pushed fully (magnitude >= 0.5). Set higher than MoveSpeed.")]
        [SerializeField, Range(1f, 30f)] private float _runSpeed = 7f;

        [Header("Animation")]
        [Tooltip("Animator Controller to use for this character.")]
        [SerializeField] private RuntimeAnimatorController _animatorController;

        // ── Public accessors ─────────────────────────────────────────────────────

        public CharacterType CharacterType     => _characterType;
        public string        DisplayName       => _displayName;
        public float         MoveSpeed         => _moveSpeed;
        public float         RunSpeed          => _runSpeed;
        public RuntimeAnimatorController AnimatorController => _animatorController;
    }
}
