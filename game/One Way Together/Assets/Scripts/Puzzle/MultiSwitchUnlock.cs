using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Activates a target GameObject only after ALL specified plate IDs have fired.
    /// Order-independent — every plate must be activated at least once.
    /// Supports 2–8 switches. Add more _plateId fields if needed.
    /// </summary>
    public class MultiSwitchUnlock : MonoBehaviour
    {
        [SerializeField] private string[]    _plateIds;
        [SerializeField] private GameObject  _target;
        [SerializeField] private float       _colliderDelay = 0.6f;

        private bool[] _pressed;

        private void Awake()
        {
            _pressed = new bool[_plateIds != null ? _plateIds.Length : 0];
        }

        private void OnEnable()  => GameEvents.OnPressurePlateChanged += HandlePlate;
        private void OnDisable() => GameEvents.OnPressurePlateChanged -= HandlePlate;

        private void HandlePlate(string id, bool active)
        {
            if (!active || _plateIds == null) return;

            for (int i = 0; i < _plateIds.Length; i++)
                if (id == _plateIds[i]) _pressed[i] = true;

            if (_target == null) return;

            // Check all pressed
            foreach (var p in _pressed)
                if (!p) return;

            _target.SetActive(true);
            var col = _target.GetComponent<Collider>();
            if (col != null) StartCoroutine(DelayCollider(col));
        }

        private System.Collections.IEnumerator DelayCollider(Collider col)
        {
            col.enabled = false;
            yield return new WaitForSeconds(_colliderDelay);
            if (col != null) col.enabled = true;
        }
    }
}
