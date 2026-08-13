using UnityEngine;

namespace OneWayTogether.Core
{
    /// <summary>Continuously rotates the GameObject around the Y axis — used for coins.</summary>
    public class SpinY : MonoBehaviour
    {
        [SerializeField] private float _degreesPerSecond = 180f;

        private void Update()
        {
            transform.Rotate(0f, _degreesPerSecond * Time.deltaTime, 0f, Space.Self);
        }
    }
}
