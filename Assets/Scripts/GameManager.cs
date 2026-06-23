using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Reference")]
    [Tooltip("The GameObject of the Game Over canvas")]
    public GameObject gameOverUI;

    [Tooltip("The TextMeshPro text component to display the progress percentage")]
    public TMPro.TextMeshProUGUI progressText;

    private void Awake()
{
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Ensure failure UI is hidden at start
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    public void GameOver(int percentage)
    {
        Debug.Log($"Game Over triggered in GameManager with {percentage}%!");
        
        // Update the progress text
        if (progressText != null)
        {
            progressText.text = $"{percentage}%";
        }

        // Show the failure UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        else
        {
            Debug.LogError("Game Over UI is not assigned in the GameManager!");
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restarting scene...");
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Revive()
    {
        Debug.Log("Revive clicked!");
        
        // Hide the failure UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        // Call revive on player
        PlayerController player = GameObject.FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.Revive();
        }
    }
}
