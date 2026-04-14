using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    // attach to player — detects if grounded via small raycast downward
    public class GroundDetector : MonoBehaviour
    {
        [SerializeField] private float checkDistance = 0.1f;
        [SerializeField] private LayerMask groundLayer;

        public bool IsGrounded { get; private set; }

        private void FixedUpdate()
        {
            IsGrounded = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                checkDistance,
                groundLayer
            );
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * checkDistance);
        }
    }
}
