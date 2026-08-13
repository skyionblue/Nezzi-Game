using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using OneWayTogether.Events;

namespace OneWayTogether.Core
{
    /// <summary>
    /// V2 city intro — plays automatically when CityWorld loads:
    ///   1. Camera starts high above the city showing the WHOLE layout.
    ///   2. After a brief hold, camera dollies down into the Puzzle 1 zone.
    ///   3. Characters are activated and gameplay begins.
    ///
    /// The main menu lives in MainMenu.unity — no UI is built here.
    /// </summary>
    public class CityIntroController : MonoBehaviour
    {
        [Header("Sky overview (shows whole city)")]
        [Tooltip("Camera position for the full-city overview shot.")]
        [SerializeField] private Vector3 _skyPosition = new Vector3(-242f, 648f, 270f);
        [SerializeField] private Vector3 _skyRotation = new Vector3(82f, 0f, 0f);
        [SerializeField] private float   _skyFOV      = 60f;
        [SerializeField] private float   _skyHoldSeconds = 1.8f;

        [Header("Puzzle 1 landing")]
        [SerializeField] private Vector3 _puzzle1LookAt = new Vector3(-100f, 22f, 287f);
        [SerializeField] private float   _puzzle1Height = 28f;
        [SerializeField] private float   _puzzle1FOV    = 52f;

        [Header("Dolly")]
        [SerializeField] private float _dollyDuration = 5.0f;
        [SerializeField] private AnimationCurve _dollyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Scene references")]
        [SerializeField] private UnityEngine.Camera _mainCamera;
        [SerializeField] private CinemachineCamera  _vcam;
        [SerializeField] private GameObject         _scarlet;
        [SerializeField] private GameObject         _dani;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            // Hide characters until the dolly lands
            if (_scarlet) _scarlet.SetActive(false);
            if (_dani)    _dani.SetActive(false);

            // Disable Cinemachine and IsometricCameraFollow so we own the camera
            if (_vcam) _vcam.enabled = false;
            if (_mainCamera)
            {
                var follow = _mainCamera.GetComponent<OneWayTogether.Camera.IsometricCameraFollow>();
                if (follow != null) follow.enabled = false;

                _mainCamera.transform.position = _skyPosition;
                _mainCamera.transform.rotation = Quaternion.Euler(_skyRotation);
                _mainCamera.fieldOfView        = _skyFOV;
            }
        }

        private void Start()
        {
            // Auto-start the dolly — no Play button needed, menu was in MainMenu.unity
            StartCoroutine(IntroDolly());
        }

        // ── Dolly coroutine ───────────────────────────────────────────────────

        private IEnumerator IntroDolly()
        {
            // Brief hold so the player can appreciate the full-city overview
            yield return new WaitForSeconds(_skyHoldSeconds);

            // Compute the landing position for Puzzle 1
            Vector3    startPos = _skyPosition;
            Quaternion startRot = Quaternion.Euler(_skyRotation);
            float      startFOV = _skyFOV;

            Vector3    endPos = _puzzle1LookAt + new Vector3(0f, _puzzle1Height, -_puzzle1Height * 0.85f);
            Quaternion endRot = Quaternion.LookRotation((_puzzle1LookAt - endPos).normalized);
            float      endFOV = _puzzle1FOV;

            // Dolly
            float t = 0f;
            while (t < _dollyDuration)
            {
                t += Time.deltaTime;
                float p = _dollyCurve.Evaluate(Mathf.Clamp01(t / _dollyDuration));

                if (_mainCamera)
                {
                    _mainCamera.transform.position = Vector3.Lerp(startPos, endPos, p);
                    _mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, p);
                    _mainCamera.fieldOfView        = Mathf.Lerp(startFOV, endFOV, p);
                }
                yield return null;
            }

            // Snap to final position
            if (_mainCamera)
            {
                _mainCamera.transform.position = endPos;
                _mainCamera.transform.rotation = endRot;
                _mainCamera.fieldOfView        = endFOV;
            }

            // Re-enable IsometricCameraFollow so it follows characters
            if (_mainCamera)
            {
                var follow = _mainCamera.GetComponent<OneWayTogether.Camera.IsometricCameraFollow>();
                if (follow != null) follow.enabled = true;
            }

            // Hand off to Cinemachine + activate characters
            if (_vcam)
            {
                _vcam.ForceCameraPosition(endPos, endRot);
                _vcam.enabled = true;
            }

            if (_scarlet) _scarlet.SetActive(true);
            if (_dani)    _dani.SetActive(true);

            GameEvents.RaiseGameStateChanged(GameState.Playing);
        }
    }
}
