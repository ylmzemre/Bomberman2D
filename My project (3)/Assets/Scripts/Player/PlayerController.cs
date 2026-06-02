using UnityEngine;
using UnityEngine.InputSystem;

namespace Bomberman2D.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float baseSpeed = 5f;
        public float currentSpeed;

        private Rigidbody2D rb;
        private Mechanics.SpriteAnimator animator;
        private Vector2 movement;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Mechanics.SpriteAnimator>();
            // Gravity is not needed in a top-down game
            rb.gravityScale = 0f;
            // Freeze rotation to prevent physics from spinning the character
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            currentSpeed = baseSpeed;
        }

        private void Update()
        {
            // Yeni Input System (Keyboard) kullanımı
            movement = Vector2.zero;
            
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) movement.y = 1;
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) movement.y = -1;
                
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) movement.x = 1;
                else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) movement.x = -1;
            }

            // Prevent diagonal movement to keep it strictly grid-like
            if (Mathf.Abs(movement.x) > 0.1f)
            {
                movement.y = 0;
            }
        }

        private void FixedUpdate()
        {
            // Apply movement
            rb.linearVelocity = movement.normalized * currentSpeed;

            if (animator != null)
            {
                animator.SetDirection(movement);
            }
        }

        // Method to increase speed when getting a power-up
        public void IncreaseSpeed(float amount)
        {
            currentSpeed += amount;
        }
    }
}
