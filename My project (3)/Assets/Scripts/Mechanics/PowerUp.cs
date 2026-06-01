using UnityEngine;
using Bomberman2D.Player;

namespace Bomberman2D.Mechanics
{
    public class PowerUp : MonoBehaviour
    {
        public enum PowerUpType
        {
            ExtraBomb,
            BlastRadius,
            Speed
        }

        public PowerUpType type;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPickedUp(other.gameObject);
            }
            // Powerups can also be destroyed by explosions
            else if (other.CompareTag("Explosion"))
            {
                Destroy(gameObject);
            }
        }

        private void OnPickedUp(GameObject player)
        {
            BombSpawner spawner = player.GetComponent<BombSpawner>();
            PlayerController controller = player.GetComponent<PlayerController>();

            switch (type)
            {
                case PowerUpType.ExtraBomb:
                    if (spawner != null) spawner.IncreaseMaxBombs(1);
                    break;
                case PowerUpType.BlastRadius:
                    if (spawner != null) spawner.IncreaseExplosionRange(1);
                    break;
                case PowerUpType.Speed:
                    if (controller != null) controller.IncreaseSpeed(1f);
                    break;
            }

            Destroy(gameObject);
        }
    }
}
