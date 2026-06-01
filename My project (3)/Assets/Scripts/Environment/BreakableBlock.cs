using UnityEngine;

namespace Bomberman2D.Environment
{
    public class BreakableBlock : MonoBehaviour
    {
        [Header("Drop Settings")]
        [Range(0f, 1f)]
        public float powerUpDropChance = 0.3f;
        public GameObject[] powerUpPrefabs;

        public void OnExploded()
        {
            // Try to drop a power-up
            if (Random.value <= powerUpDropChance && powerUpPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, powerUpPrefabs.Length);
                Instantiate(powerUpPrefabs[randomIndex], transform.position, Quaternion.identity);
            }

            // Destroy the block
            Destroy(gameObject);
        }
    }
}
