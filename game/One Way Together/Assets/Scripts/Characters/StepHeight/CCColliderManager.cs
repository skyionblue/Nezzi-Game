using System.Collections.Generic;
using LB.Player.Movement.StepHeight;
using UnityEngine;

namespace OneWayTogether.Characters.StepHeight
{
    /// <summary>
    /// Adapts a CharacterController's collider geometry to the IColliderManager interface
    /// expected by StepHeightController.
    ///
    /// All physics queries mirror the patterns in the package's ColliderManager, substituting
    /// CharacterController.radius and CharacterController.height for the original bounds-based
    /// calculation over an arbitrary Collider[]. This avoids LINQ and unnecessary allocations
    /// in the per-frame overlap check.
    ///
    /// layersToIgnore should be set to the Character layer in the Inspector so the
    /// OverlapSphere never reports the character itself as a contact point.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class CCColliderManager : MonoBehaviour, IColliderManager
    {
        [Tooltip("Physics layers that should be excluded from step-detection queries. " +
                 "Always include the Character layer.")]
        [SerializeField] private LayerMask _layersToIgnore;

        private CharacterController _cc;

        // Cached values written by CachePlayerColliderInfo
        private float _cachedRadius;
        private float _cachedHeight;

        // Reused buffer — avoids per-frame allocation in CollectContactPointsUsingOverlapSphere.
        private readonly Collider[] _overlapBuffer = new Collider[16];

        // Backing list for ContactPoints; allocated once and cleared each query.
        private readonly List<MyContactPoint> _contactPoints = new List<MyContactPoint>(8);

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            CachePlayerColliderInfo();
        }

        // IColliderManager ────────────────────────────────────────────────────────

        /// <summary>
        /// Snapshot of contact points from the most recent call to
        /// <see cref="CollectContactPointsUsingOverlapSphere"/>.
        /// </summary>
        public List<MyContactPoint> ContactPoints => _contactPoints;

        /// <summary>
        /// Stores CharacterController.radius and effective world-space height so
        /// subsequent queries never re-read the component. Call after teleports or
        /// scale changes via StepHeightController.UpdateCachedPlayerColliderInfo().
        /// </summary>
        public void CachePlayerColliderInfo()
        {
            _cachedRadius = _cc.radius;
            // CharacterController.height is the full capsule height in local space.
            // Multiply by lossyScale.y to convert to world-space.
            _cachedHeight = _cc.height * transform.lossyScale.y;
        }

        /// <summary>
        /// Fills ContactPoints with nearby non-player, non-trigger colliders using an
        /// OverlapSphereNonAlloc so no heap allocation occurs per call.
        /// </summary>
        public void CollectContactPointsUsingOverlapSphere()
        {
            _contactPoints.Clear();

            // Use the capsule's centre in world space as the sphere origin.
            Vector3 centre = transform.position + _cc.center;
            // The package uses a fixed 0.6-unit radius for overlap — preserve that intent
            // but grow it proportionally if the character's actual radius is larger.
            float queryRadius = Mathf.Max(0.6f, _cachedRadius * 1.2f);

            int mask = ~_layersToIgnore;
            int hitCount = Physics.OverlapSphereNonAlloc(
                centre, queryRadius, _overlapBuffer, mask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider other = _overlapBuffer[i];
                if (other == null || other.isTrigger) continue;

                // Closest point on the obstacle to the character's centre.
                Vector3 closest = other.ClosestPoint(transform.position);
                // Lift slightly above ground to avoid precision-boundary false negatives
                // (same adjustment as the reference ColliderManager).
                closest.y = transform.position.y + 0.01f;

                _contactPoints.Add(new MyContactPoint
                {
                    Point = closest,
                    OtherCollider = other
                });
            }
        }

        /// <summary>
        /// Returns true if an OverlapSphere at <paramref name="position"/> with the given
        /// <paramref name="radius"/> hits any non-player solid collider.
        /// Used by StepHeightController to confirm the top of a step is clear.
        /// </summary>
        public bool IsInsideCollider(Vector3 position, float radius)
        {
            int mask = ~_layersToIgnore;
            int hitCount = Physics.OverlapSphereNonAlloc(
                position, radius, _overlapBuffer, mask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider other = _overlapBuffer[i];
                if (other == null || other.isTrigger) continue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Fires a grid of downward raycasts around <paramref name="groundPosition"/> and
        /// returns the highest hit point, which becomes the step landing height.
        /// </summary>
        public Vector3 GetHighestGroundPoint(Vector3 groundPosition, float height, float radius)
        {
            // Nine-point grid identical to ColliderManager's implementation.
            float half = radius * 0.5f;
            Vector3[] offsets =
            {
                Vector3.zero,
                new Vector3(radius,  0f, 0f),
                new Vector3(-radius, 0f, 0f),
                new Vector3(0f,      0f,  radius),
                new Vector3(0f,      0f, -radius),
                new Vector3(half,    0f,  half),
                new Vector3(-half,   0f,  half),
                new Vector3(half,    0f, -half),
                new Vector3(-half,   0f, -half)
            };

            foreach (Vector3 offset in offsets)
            {
                Vector3 rayOrigin = transform.position + offset + Vector3.up * 0.1f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, height,
                        ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.point.y > groundPosition.y)
                        groundPosition = hit.point;
                }
            }

            return groundPosition;
        }

        /// <summary>
        /// Returns true if a SphereCast upward from <paramref name="stepUpPosition"/> would
        /// hit a ceiling before the character could fully stand. Prevents stepping into
        /// overhangs or enclosed spaces.
        /// </summary>
        public bool HasCeilingCollision(Vector3 stepUpPosition, float radius, float height)
        {
            // Reuse a stack-allocated buffer for the non-alloc cast.
            RaycastHit[] hits = new RaycastHit[8];
            Vector3 start = stepUpPosition + Vector3.up * (height * 0.5f);
            int hitCount = Physics.SphereCastNonAlloc(
                start, radius, Vector3.up, hits, radius * 2f, ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider c = hits[i].collider;
                if (c == null || c.isTrigger) continue;
                // Any non-trigger hit above the step position is a ceiling.
                return true;
            }

            return false;
        }

        /// <summary>CharacterController radius in world space.</summary>
        public float GetCachedPlayerColliderRadius() => _cachedRadius;

        /// <summary>CharacterController full height in world space.</summary>
        public float GetCachedPlayerColliderHeight() => _cachedHeight;
    }
}
