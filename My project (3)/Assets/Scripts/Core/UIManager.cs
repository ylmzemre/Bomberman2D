using UnityEngine;
using UnityEngine.UI;
using Bomberman2D.Core;
using Bomberman2D.Player;
using TMPro; // Unity's modern text component

namespace Bomberman2D.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI timeText;
        public TextMeshProUGUI enemyCountText;
        public TextMeshProUGUI bombCountText;
        
        [Header("Lives Container")]
        public Transform livesContainer;
        public GameObject heartIconPrefab; // Prefab with an Image component showing the Heart.png

        private PlayerController player;
        private BombSpawner spawner;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsUpdated += UpdateUI;
            }

            // Find player to read bomb count
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null)
            {
                player = pObj.GetComponent<PlayerController>();
                spawner = pObj.GetComponent<BombSpawner>();
            }

            UpdateUI();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsUpdated -= UpdateUI;
            }
        }

        private void UpdateUI()
        {
            if (GameManager.Instance == null) return;

            // Update Texts
            if (scoreText != null) scoreText.text = $"Score: {GameManager.Instance.score}";
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(GameManager.Instance.timeRemaining / 60);
                int seconds = Mathf.FloorToInt(GameManager.Instance.timeRemaining % 60);
                timeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
            if (enemyCountText != null) enemyCountText.text = $"Enemies: {GameManager.Instance.totalEnemies}";
            
            if (bombCountText != null && spawner != null) 
            {
                int availableBombs = spawner.maxBombs - spawner.currentBombsCount;
                bombCountText.text = $"Bombs: {availableBombs}/{spawner.maxBombs}";
            }

            // Update Lives (Hearts)
            if (livesContainer != null && heartIconPrefab != null)
            {
                // Clear existing hearts
                foreach (Transform child in livesContainer)
                {
                    Destroy(child.gameObject);
                }

                // Instantiate hearts based on lives
                for (int i = 0; i < GameManager.Instance.lives; i++)
                {
                    Instantiate(heartIconPrefab, livesContainer);
                }
            }
        }
    }
}
