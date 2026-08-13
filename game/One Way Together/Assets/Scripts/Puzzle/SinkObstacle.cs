using System.Collections;
using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Lowers a blocking prop below the ground when its plate ID fires (one-shot).
    /// Disables blocking colliders immediately on activation so characters can cross.
    /// Used for city bollards/barriers that retract into the pavement.
    /// </summary>
    public class SinkObstacle : MonoBehaviour
    {
        [SerializeField] private string  _plateId      = "Plate_01";
        [SerializeField] private float   _sinkDepth    = 6f;
        [SerializeField] private float   _sinkSpeed    = 4f;

        private Vector3 _raisedPos;
        private Vector3 _sunkenPos;
        private bool    _sunk;

        private void Awake()
        {
            _raisedPos = transform.position;
            _sunkenPos = _raisedPos + Vector3.down * _sinkDepth;
        }

        private void OnEnable()  => GameEvents.OnPressurePlateChanged += HandlePlate;
        private void OnDisable() => GameEvents.OnPressurePlateChanged -= HandlePlate;

        private void HandlePlate(string id, bool active)
        {
            if (id != _plateId || _sunk || !active) return;
            _sunk = true;
            // Disable all colliders immediately so characters can walk through
            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = false;
            StartCoroutine(SinkDown());
        }

        private IEnumerator SinkDown()
        {
            while (transform.position.y > _sunkenPos.y + 0.02f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, _sunkenPos, _sinkSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = _sunkenPos;
        }
    }
}
