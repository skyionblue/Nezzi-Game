using UnityEngine;
using OneWayTogether.Characters;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Invisible trigger zone placed on rope and vine GameObjects.
    /// When Dani enters, her <see cref="DaniController.SetClimbingState"/> is enabled.
    /// When she exits, climbing is disabled and normal gravity resumes.
    ///
    /// Uses 3D trigger callbacks (OnTriggerEnter/Exit) because CharacterController
    /// automatically creates a 3D CapsuleCollider — 2D physics is not used in
    /// the HD-2D isometric build.
    ///
    /// Note: only Dani can climb — the check uses the DaniController component.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RopeTrigger : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            DaniController dani = other.GetComponentInParent<DaniController>();
            dani?.SetClimbingState(true);
        }

        private void OnTriggerExit(Collider other)
        {
            DaniController dani = other.GetComponentInParent<DaniController>();
            dani?.SetClimbingState(false);
        }
    }
}
