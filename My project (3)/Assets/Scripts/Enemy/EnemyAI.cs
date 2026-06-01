using UnityEngine;
using Bomberman2D.Core;

namespace Bomberman2D.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAI : MonoBehaviour
    {
        public float speed = 2f;
        private Rigidbody2D rb;
        private Vector2 moveDirection;
        public LayerMask obstacleMask;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            // Start moving in a random cardinal direction
            PickRandomDirection();
            
            // Register with GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemy();
            }
        }

        private void FixedUpdate()
        {
            rb.velocity = moveDirection * speed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // If hitting a wall, block or another enemy, pick a new direction
            if (collision.gameObject.CompareTag("Wall") || 
                collision.gameObject.CompareTag("Breakable") || 
                collision.gameObject.CompareTag("Bomb") ||
                collision.gameObject.CompareTag("Enemy"))
            {
                PickRandomDirection();
            }
            // Damage player
            else if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player touched enemy! Game Over.");
                Destroy(collision.gameObject);
                // GameManager.GameOver();
            }
        }

        private void PickRandomDirection()
        {
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            moveDirection = directions[Random.Range(0, directions.Length)];
        }

        private void OnDestroy()
        {
            // Unregister with GameManager when destroyed (killed by explosion)
            if (GameManager.Instance != null && gameObject.scene.isLoaded)
            {
                GameManager.Instance.UnregisterEnemy();
            }
        }
    }
}
