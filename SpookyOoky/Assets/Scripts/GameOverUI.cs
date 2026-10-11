
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    private void Awake()
    {
        // Make sure the game starts normally.
        Time.timeScale = 1f;

        // Hide the Game Over screen at the start.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // Called when the player's health reaches zero.
    public void ShowGameOver()
    {
        // Show the Game Over screen.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Unlock the cursor so we can click Restart.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pause the game.
        Time.timeScale = 0f;
    }

    // Called by the Restart button.
    public void RestartGame()
    {
        // Unpause before restarting.
        Time.timeScale = 1f;

        // Reload the current scene.
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
