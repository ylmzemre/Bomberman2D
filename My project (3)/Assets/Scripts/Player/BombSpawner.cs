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
                // Grid'deki koordinatını hesapla (Ölçekler 0.5 olduğu için 0.5'in katlarına yuvarla)
                // Grid cell size 1, ama scale 0.5 olduğu için grid koordinatları 0.5 adımlarla ilerler
                // Fakat biz basitçe Tilemap Grid'ine uydurmak için Mathf.Round(pos * 2f) / 2f yapabiliriz
                // Grid ayarları değişirse burayı güncellemek gerekebilir. Şu anki gridde 0.5'in katları uygundur.
                Vector2 placePos = new Vector2(
                    Mathf.Round(transform.position.x * 2f) / 2f,
                    Mathf.Round(transform.position.y * 2f) / 2f
                );

                // O noktada Duvar, Kutu veya Bomba var mı kontrol et
                Collider2D[] colliders = Physics2D.OverlapCircleAll(placePos, 0.1f);
                foreach (var hit in colliders)
                {
                    if (hit.CompareTag("Bomb") || hit.CompareTag("Wall") || hit.CompareTag("Breakable"))
                    {
                        return; // O alanda bir engel varsa bomba koyma!
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
