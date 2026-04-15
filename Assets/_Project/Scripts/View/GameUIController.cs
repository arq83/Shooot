using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    public Slider hpBar;           
    //public TextMeshProUGUI hpText;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText; 

    private PlayerRuntimeData player;
    private GameState state;

    public void Init(PlayerRuntimeData player, GameState state)
    {
        this.player = player;
        this.state = state;

        if (hpBar)
        {
            hpBar.maxValue = player.MaxHealth;
            hpBar.value = player.Health;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Game");
    }

    void Update()
    {
        if (player == null) return;

        // HP
        if (hpBar) hpBar.value = Mathf.MoveTowards(hpBar.value, player.Health, Time.deltaTime * 20f);
        //if (hpText) hpText.text = player.Health + " / " + player.MaxHealth;

        // Score
        if (scoreText) scoreText.text = "Score: " + state.Score;

        // Game Over
        if (state.IsGameOver && !gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText) finalScoreText.text = "Score: " + state.Score + "\nBest: " + HighscoreSystem.Get();
            SaveSystem.Save(new SaveData { playerHealth = 0, wave = 0 });
        }
    }
}