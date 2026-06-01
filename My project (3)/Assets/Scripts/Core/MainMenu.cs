using UnityEngine;
using UnityEngine.UI;

namespace Bomberman2D.UI
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject topPanel; // Oyun içi UI paneli
        public GameObject demoParent; // Sahnede bekleyen objeleri aktif etmek için (isteğe bağlı)

        public void PlayGame()
        {
            // Ana menüyü gizle, oyun UI'ını aç
            gameObject.SetActive(false);
            if (topPanel != null) topPanel.SetActive(true);
            
            // Oyunu başlatmak için GameManager veya ilgili kısımlar tetiklenebilir
            Debug.Log("Oyun Başladı!");
            Time.timeScale = 1f; // Oyun başladıktan sonra zamanı akıt
        }

        public void QuitGame()
        {
            Debug.Log("Quit Game...");
            Application.Quit();
        }
    }
}
