using UnityEngine;
using UnityEngine.UI;

namespace Bomberman2D.UI
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject topPanel; // Oyun içi UI paneli
        public Button playButton; // Tıklanacak Start butonu

        private void Start()
        {
            // Oyun başladığında zamanı durdur
            Time.timeScale = 0f;
            
            // Çalışma zamanında (runtime) butona tıklama olayını ekle
            if (playButton != null)
            {
                playButton.onClick.AddListener(PlayGame);
            }
        }

        public void PlayGame()
        {
            // Ana menüyü gizle, oyun UI'ını aç
            gameObject.SetActive(false);
            if (topPanel != null) topPanel.SetActive(true);
            
            // Oyunu başlatmak için zamanı normal hızına al
            Debug.Log("Oyun Başladı!");
            Time.timeScale = 1f; 
        }

        public void QuitGame()
        {
            Debug.Log("Quit Game...");
            Application.Quit();
        }
    }
}
