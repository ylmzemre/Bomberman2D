using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace Bomberman2D.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Stats")]
        public int score = 0;
        public int lives = 3;
        public float timeRemaining = 300f; // 5 minutes
        public int totalEnemies = 0;

        private bool isGameOver = false;

        public event Action OnStatsUpdated;

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

        private void Update()
        {
            if (!isGameOver && timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0)
                {
                    timeRemaining = 0;
                    GameOver();
                }
                OnStatsUpdated?.Invoke();
            }
        }

        public void AddScore(int amount)
        {
            score += amount;
            OnStatsUpdated?.Invoke();
        }

        public void RegisterEnemy()
        {
            totalEnemies++;
            OnStatsUpdated?.Invoke();
        }

        public void UnregisterEnemy()
        {
            totalEnemies--;
            OnStatsUpdated?.Invoke();
            
            if (totalEnemies <= 0 && !isGameOver)
            {
                LevelComplete();
            }
        }

        public void PlayerDied()
        {
            if (isGameOver) return;
            
            lives--;
            OnStatsUpdated?.Invoke();
            
            if (lives <= 0)
            {
                GameOver();
            }
            else
            {
                Debug.Log("Player died. Restarting level with remaining lives...");
                Invoke("RestartLevel", 2f);
            }
        }

        public void LevelComplete()
        {
            if (isGameOver) return;
            isGameOver = true;
            Debug.Log("Level Complete! All enemies defeated.");
            // Wait a bit, then you could load the next level or Main Menu
            Invoke("RestartLevel", 3f);
        }

        public void GameOver()
        {
            if (isGameOver) return;
            isGameOver = true;
            Debug.Log("Game Over!");
            Invoke("LoadMainMenu", 3f);
        }

        private void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
