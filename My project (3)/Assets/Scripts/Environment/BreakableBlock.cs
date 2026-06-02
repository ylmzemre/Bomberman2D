using UnityEngine;

namespace Bomberman2D.Environment
{
    public class BreakableBlock : MonoBehaviour
    {
        public GameObject[] powerupPrefabs;

        public void OnExploded()
        {
            // %30 şansla güçlendirme düşür
            if (powerupPrefabs != null && powerupPrefabs.Length > 0 && Random.value < 0.3f)
            {
                int index = Random.Range(0, powerupPrefabs.Length);
                Instantiate(powerupPrefabs[index], transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Fallback: Kutuyu sadece patlama kırabilir
            if (collision.CompareTag("Explosion"))
            {
                OnExploded();
            }
        }
    }
}
