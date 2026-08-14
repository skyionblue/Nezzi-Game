using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Activates a target GameObject only after TWO specific plate IDs have both fired.
    /// Order doesn't matter — both must be activated at least once before the target unlocks.
    /// Used for Puzzle 4: lift Dani to press two high switches, then reunion appears.
    /// </summary>
    public class TwoSwitchUnlock : MonoBehaviour
    {
        [SerializeField] private string     _plateIdA;
        [SerializeField] private string     _plateIdB;
        [SerializeField] private GameObject _target;
        [SerializeField] private float      _colliderDelay = 0.6f;

        private bool _aPressed;
        private bool _bPressed;

        private void OnEnable()  => GameEvents.OnPressurePlateChanged += HandlePlate;
        private void OnDisable() => GameEvents.OnPressurePlateChanged -= HandlePlate;

        private void HandlePlate(string id, bool active)
        {
            if (!active) return;
            if (id == _plateIdA) _aPressed = true;
            if (id == _plateIdB) _bPressed = true;

            if (_aPressed && _bPressed && _target != null)
            {
                _target.SetActive(true);
                var col = _target.GetComponent<Collider>();
                if (col != null) StartCoroutine(DelayCollider(col));
            }
        }

        private System.Collections.IEnumerator DelayCollider(Collider col)
        {
            col.enabled = false;
            yield return new WaitForSeconds(_colliderDelay);
            if (col != null) col.enabled = true;
        }
    }
}
