using UnityEngine;

namespace Bomberman2D.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float baseSpeed = 5f;
        public float currentSpeed;

        private Rigidbody2D rb;
        private Vector2 movement;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            // Gravity is not needed in a top-down game
            rb.gravityScale = 0f;
            // Freeze rotation to prevent physics from spinning the character
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            currentSpeed = baseSpeed;
        }

        private void Update()
        {
            // Get raw input to avoid floaty movement (classic bomberman movement is usually snappy)
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            // Prevent diagonal movement to keep it strictly grid-like, 
            // or allow it but normalize it. Classic bomberman usually locks you to one axis at a time.
            if (Mathf.Abs(movement.x) > 0.1f)
            {
                movement.y = 0;
            }
        }

        private void FixedUpdate()
        {
            // Apply movement
            rb.linearVelocity = movement.normalized * currentSpeed;
        }

        // Method to increase speed when getting a power-up
        public void IncreaseSpeed(float amount)
        {
            currentSpeed += amount;
        }
    }
}
