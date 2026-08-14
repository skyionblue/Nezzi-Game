using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Flashes a fullscreen red screen-space overlay when a character hits an
    /// invisible boundary wall. No world-space mesh is created — the mesh-based
    /// approach caused pink/error rendering on Android due to shader stripping.
    /// Called from CharacterBase.OnControllerColliderHit.
    /// </summary>
    public class WallFlash : MonoBehaviour
    {
        [SerializeField] private float _flashDuration = 0.4f;
        [SerializeField] private float _peakAlpha     = 0.35f;

        private bool _flashing;

        // ── Shared screen-space overlay (same approach as the old fullscreen flash) ──

        private static Image  s_overlay;
        private static Canvas s_canvas;

        private static void EnsureOverlay()
        {
            if (s_overlay != null) return;

            var canvasGO = new GameObject("WallFlashCanvas");
            Object.DontDestroyOnLoad(canvasGO);

            s_canvas = canvasGO.AddComponent<Canvas>();
            s_canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            s_canvas.sortingOrder = 200;
            canvasGO.AddComponent<CanvasScaler>();

            var imgGO = new GameObject("WallFlashOverlay");
            imgGO.transform.SetParent(canvasGO.transform, false);

            var rect = imgGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            s_overlay = imgGO.AddComponent<Image>();
            s_overlay.color        = new Color(1f, 0f, 0f, 0f);
            s_overlay.raycastTarget = false;
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake() => EnsureOverlay();

        // ── Public API ────────────────────────────────────────────────────────────

        public void Flash()
        {
            if (_flashing) return;
            StopAllCoroutines();
            StartCoroutine(DoFlash());
        }

        // ── Internals ─────────────────────────────────────────────────────────────

        private IEnumerator DoFlash()
        {
            _flashing = true;

            if (s_overlay != null)
                s_overlay.color = new Color(1f, 0.05f, 0.05f, _peakAlpha);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / _flashDuration;
                if (s_overlay != null)
                    s_overlay.color = new Color(1f, 0.05f, 0.05f, Mathf.Lerp(_peakAlpha, 0f, t));
                yield return null;
            }

            if (s_overlay != null)
                s_overlay.color = new Color(1f, 0f, 0f, 0f);

            _flashing = false;
        }
    }
}
