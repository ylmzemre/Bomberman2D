using UnityEngine;
using System.Collections;
using Bomberman2D.Player;
using Bomberman2D.Core;

namespace Bomberman2D.Mechanics
{
    public class Bomb : MonoBehaviour
    {
        [Header("Explosion Settings")]
        public GameObject explosionPrefab;
        public float fuseTime = 3f;
        public LayerMask levelMask; // Layer for walls/blocks to stop raycasts
        
        private BombSpawner spawner;
        private int range;
        private bool exploded = false;
        private Collider2D bombCollider;

        private void Start()
        {
            bombCollider = GetComponent<Collider2D>();
            if (bombCollider != null)
            {
                // Bomba ilk koyulduğunda geçirgen (Trigger) olsun ki oyuncu sıkışmasın
                bombCollider.isTrigger = true;
            }
        }

        public void Initialize(BombSpawner spawner, int range)
        {
            this.spawner = spawner;
            this.range = range;
            Invoke("Explode", fuseTime);
        }

        private void Explode()
        {
            if (exploded) return;
            exploded = true;

            // Free up the player's bomb count
            if (spawner != null)
            {
                spawner.OnBombExploded();
            }

            // Center explosion
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosion();

            // Explosions in 4 directions
            StartCoroutine(CreateExplosions(Vector2.up));
            StartCoroutine(CreateExplosions(Vector2.down));
            StartCoroutine(CreateExplosions(Vector2.right));
            StartCoroutine(CreateExplosions(Vector2.left));

            // Hide the bomb sprite/collider immediately
            GetComponent<SpriteRenderer>().enabled = false;
            if (bombCollider != null) bombCollider.enabled = false;

            // Destroy this bomb object shortly after
            Destroy(gameObject, 0.5f);
        }

        private IEnumerator CreateExplosions(Vector2 direction)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2 pos = (Vector2)transform.position + (direction * i);

                // Raycast to check if there is a wall or block at the target position
                // We use a small overlap circle to detect colliders in the grid cell
                Collider2D hit = Physics2D.OverlapCircle(pos, 0.1f, levelMask);

                if (hit != null)
                {
                    // If we hit an unbreakable wall, stop expanding in this direction
                    if (hit.CompareTag("Wall"))
                    {
                        break; 
                    }
                    
                    // If we hit a breakable block, create an explosion on it, and then stop expanding
                    if (hit.CompareTag("Breakable"))
                    {
                        Instantiate(explosionPrefab, pos, Quaternion.identity);
                        // The explosion itself will handle destroying the block via triggers or physics overlap
                        break;
                    }
                }

                // If nothing blocks it, instantiate an explosion and continue loop
                Instantiate(explosionPrefab, pos, Quaternion.identity);

                // Small delay to make explosion travel fast rather than instant (optional, usually instant in classic bomberman)
                yield return new WaitForSeconds(0.05f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Chain reactions: if an explosion hits this bomb, explode immediately
            if (!exploded && other.CompareTag("Explosion"))
            {
                CancelInvoke("Explode");
                Explode();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Oyuncu bombanın üzerinden çekildiğinde bombayı katı hale getir
            if (other.CompareTag("Player") && bombCollider != null)
            {
                bombCollider.isTrigger = false;
            }
        }
    }
}
