using UnityEngine;
using OneWayTogether.Characters;

namespace OneWayTogether.Puzzle
{
    /// <summary>
    /// Trigger zone placed inside narrow passages. When Dani enters, her
    /// Crawl animator bool is set to true so the crawl clip plays. When she
    /// exits, the bool is cleared and she returns to Idle/Walk/Run.
    ///
    /// Only Dani can crawl — Scarlet entering the same trigger is ignored.
    /// Attach to a GameObject with a trigger Collider sized to match the
    /// passage interior.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CrawlTrigger : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            DaniController dani = other.GetComponentInParent<DaniController>();
            dani?.SetCrawlingState(true);
        }

        private void OnTriggerExit(Collider other)
        {
            DaniController dani = other.GetComponentInParent<DaniController>();
            dani?.SetCrawlingState(false);
        }
    }
}
