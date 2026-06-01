using UnityEngine;

namespace Bomberman2D.Mechanics
{
    public class Explosion : MonoBehaviour
    {
        [Header("Explosion Settings")]
        public float duration = 0.5f;

        private void Start()
        {
            // Automatically destroy the explosion visual/hitbox after a short time
            Destroy(gameObject, duration);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Handle player death
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player hit by explosion!");
                if (Bomberman2D.Core.GameManager.Instance != null)
                {
                    Bomberman2D.Core.GameManager.Instance.PlayerDied();
                }
                Destroy(other.gameObject);
            }
            
            // Handle enemy death
            else if (other.CompareTag("Enemy"))
            {
                Debug.Log("Enemy hit by explosion!");
                // Add score via GameManager
                if (Bomberman2D.Core.GameManager.Instance != null)
                {
                    Bomberman2D.Core.GameManager.Instance.AddScore(100);
                }
                Destroy(other.gameObject);
            }
            
            // Handle breakable block destruction
            else if (other.CompareTag("Breakable"))
            {
                // We will add a method on the block to destroy it and drop powerups
                other.SendMessage("OnExploded", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
