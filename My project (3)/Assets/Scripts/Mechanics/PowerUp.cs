using UnityEngine;
using Bomberman2D.Player;
using Bomberman2D.Core;

namespace Bomberman2D.Mechanics
{
    public enum PowerUpType { BombCount, FireRange, Speed }

    public class PowerUp : MonoBehaviour
    {
        public PowerUpType type;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                BombSpawner spawner = collision.GetComponent<BombSpawner>();

                if (type == PowerUpType.Speed && player != null)
                {
                    player.currentSpeed += 1.5f;
                }
                else if (type == PowerUpType.BombCount && spawner != null)
                {
                    spawner.maxBombs++;
                }
                else if (type == PowerUpType.FireRange && spawner != null)
                {
                    spawner.bombRange++;
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayClick(); // Şimdilik powerup sesi olarak
                }

                Destroy(gameObject);
            }
        }
    }
}
