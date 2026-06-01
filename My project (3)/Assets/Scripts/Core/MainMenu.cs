using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bomberman2D.UI
{
    public class MainMenu : MonoBehaviour
    {
        public void PlayGame()
        {
            // Assuming the first game level is at build index 1
            SceneManager.LoadScene(1);
        }

        public void QuitGame()
        {
            Debug.Log("Quit Game...");
            Application.Quit();
        }
    }
}
