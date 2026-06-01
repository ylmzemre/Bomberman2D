using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Bomberman2D.Mechanics;

namespace Bomberman2D.Player
{
    public class BombSpawner : MonoBehaviour
    {
        [Header("Bomb Settings")]
        public GameObject bombPrefab;
        
        [Header("Player Stats")]
        public int maxBombs = 1;
        public int currentBombsCount = 0;
        public int explosionRange = 1;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TryPlaceBomb();
            }
        }

        private void TryPlaceBomb()
        {
            if (currentBombsCount < maxBombs)
            {
                // In Bomberman, bombs typically snap to the center of the grid tile
                Vector2 placePos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
                
                // Check if a bomb already exists at this location
                Collider2D[] colliders = Physics2D.OverlapCircleAll(placePos, 0.1f);
                foreach (var col in colliders)
                {
                    if (col.CompareTag("Bomb"))
                    {
                        return; // Already a bomb here
                    }
                }

                if (bombPrefab != null)
                {
                    currentBombsCount++;
                    GameObject newBomb = Instantiate(bombPrefab, placePos, Quaternion.identity);
                    
                    Bomb bombComponent = newBomb.GetComponent<Bomb>();
                    if (bombComponent != null)
                    {
                        bombComponent.Initialize(this, explosionRange);
                    }
                }
            }
        }

        public void OnBombExploded()
        {
            currentBombsCount--;
            if (currentBombsCount < 0) currentBombsCount = 0;
        }

        public void IncreaseMaxBombs(int amount)
        {
            maxBombs += amount;
        }

        public void IncreaseExplosionRange(int amount)
        {
            explosionRange += amount;
        }
    }
}
