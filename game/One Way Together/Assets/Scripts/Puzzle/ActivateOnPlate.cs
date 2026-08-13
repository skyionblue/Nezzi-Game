using System.Collections;
using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Activates a target GameObject when a plate ID fires active (one-shot).
    /// Briefly disables the target's Collider after activation to avoid an
    /// immediate OnTriggerEnter if characters are already inside the bounds
    /// (happens when the ReunionTrigger is revealed mid-puzzle).
    /// </summary>
    public class ActivateOnPlate : MonoBehaviour
    {
        [SerializeField] private string     _plateId;
        [SerializeField] private GameObject _target;
        [SerializeField] private float      _colliderEnableDelay = 0.6f;

        private void OnEnable()  => GameEvents.OnPressurePlateChanged += HandlePlate;
        private void OnDisable() => GameEvents.OnPressurePlateChanged -= HandlePlate;

        private void HandlePlate(string id, bool active)
        {
            if (id != _plateId || !active || _target == null) return;

            _target.SetActive(true);

            // Disable the collider briefly so the physics system doesn't fire
            // OnTriggerEnter for characters that were already in the bounds area.
            var col = _target.GetComponent<Collider>();
            if (col != null) StartCoroutine(DelayCollider(col));
        }

        private IEnumerator DelayCollider(Collider col)
        {
            col.enabled = false;
            yield return new WaitForSeconds(_colliderEnableDelay);
            if (col != null) col.enabled = true;
        }
    }
}
