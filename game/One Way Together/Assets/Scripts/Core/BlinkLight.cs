using System.Collections;
using UnityEngine;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Pulses a child Point light on/off to indicate an interactable prop.
    /// Also vibrates the device as the active character approaches — quickening
    /// pulse rate the closer they get, like a metal-detector signal.
    /// </summary>
    public class BlinkLight : MonoBehaviour
    {
        [Header("Blink")]
        [SerializeField] private Light  _light;
        [SerializeField] private float  _onDuration  = 0.15f;
        [SerializeField] private float  _offDuration = 1.1f;
        [SerializeField] private Color  _color       = new Color(0.1f, 1f, 0.15f);
        [SerializeField] private float  _intensity   = 3f;

        [Header("Proximity Haptic")]
        [SerializeField] private float _maxRange      = 10f;
        [SerializeField] private float _minRange      = 2f;
        [SerializeField] private float _intervalFar   = 2.5f;
        [SerializeField] private float _intervalClose = 0.18f;

        private Transform _scarlet;
        private Transform _dani;
        private float     _nextPulse;
        private bool      _playing;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_light == null) _light = GetComponentInChildren<Light>();
        }

        private void Start()
        {
            var s = GameObject.FindWithTag("Scarlet");
            var d = GameObject.FindWithTag("Dani");
            if (s != null) _scarlet = s.transform;
            if (d != null) _dani    = d.transform;
        }

        private void OnEnable()
        {
            if (_light != null) _light.intensity = 0f;
            GameEvents.OnGameStateChanged += HandleState;
            StartCoroutine(Blink());
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleState;
            StopAllCoroutines();
        }

        private void HandleState(GameState s) => _playing = s == GameState.Playing;

        // ── Blink ─────────────────────────────────────────────────────────────────

        private IEnumerator Blink()
        {
            while (true)
            {
                if (_light != null) { _light.color = _color; _light.intensity = _intensity; }
                yield return new WaitForSeconds(_onDuration);
                if (_light != null) _light.intensity = 0f;
                yield return new WaitForSeconds(_offDuration);
            }
        }

        // ── Proximity haptic ──────────────────────────────────────────────────────

        private void Update()
        {
            if (!_playing || Time.time < _nextPulse) return;

            float closest = ClosestDistance();
            if (closest > _maxRange) return;

            float t        = 1f - Mathf.InverseLerp(_minRange, _maxRange, closest);
            float interval = Mathf.Lerp(_intervalFar, _intervalClose, t);
            _nextPulse = Time.time + interval;

#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        private float ClosestDistance()
        {
            float d = float.MaxValue;
            if (_scarlet != null && _scarlet.gameObject.activeInHierarchy)
                d = Mathf.Min(d, Vector3.Distance(transform.position, _scarlet.position));
            if (_dani != null && _dani.gameObject.activeInHierarchy)
                d = Mathf.Min(d, Vector3.Distance(transform.position, _dani.position));
            return d;
        }
    }
}
