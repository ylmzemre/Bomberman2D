using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bomberman2D.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int totalEnemies = 0;
        private bool isGameOver = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterEnemy()
        {
            totalEnemies++;
        }

        public void UnregisterEnemy()
        {
            totalEnemies--;
            if (totalEnemies <= 0 && !isGameOver)
            {
                LevelComplete();
            }
        }

        public void LevelComplete()
        {
            if (isGameOver) return;
            isGameOver = true;
            Debug.Log("Level Complete! All enemies defeated.");
            // Proceed to next level or show victory screen
            Invoke("RestartLevel", 3f);
        }

        public void GameOver()
        {
            if (isGameOver) return;
            isGameOver = true;
            Debug.Log("Game Over!");
            Invoke("RestartLevel", 3f);
        }

        private void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
