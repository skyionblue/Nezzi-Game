using UnityEngine;
using OneWayTogether.Characters;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Invisible trigger zone placed on rope and vine GameObjects.
    /// When Dani enters, her <see cref="DaniController.SetClimbingState"/> is enabled.
    /// When she exits, climbing is disabled and normal gravity resumes.
    ///
    /// Note: only Dani can climb — the check uses the DaniController component.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RopeTrigger : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            DaniController dani = other.GetComponentInParent<DaniController>();
            dani?.SetClimbingState(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            DaniController dani = other.GetComponentInParent<DaniController>();
            dani?.SetClimbingState(false);
        }
    }
}
