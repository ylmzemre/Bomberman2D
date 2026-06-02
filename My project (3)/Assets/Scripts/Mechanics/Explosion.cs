using UnityEngine;

namespace Bomberman2D.Mechanics
{
    public class Explosion : MonoBehaviour
    {
        public Sprite[] frames;
        public float frameRate = 0.05f;

        private SpriteRenderer sr;
        private float timer;
        private int currentFrame;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            
            if (frames == null || frames.Length == 0)
            {
                Destroy(gameObject, 0.5f); // Fallback destroy if no frames
            }
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            timer += Time.deltaTime;
            if (timer >= frameRate)
            {
                timer -= frameRate;
                currentFrame++;
                if (currentFrame < frames.Length)
                {
                    sr.sprite = frames[currentFrame];
                }
                else
                {
                    Destroy(gameObject); // Animasyon bitince yok ol
                }
            }
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
