using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public CanvasGroup mainPanel;
    public CanvasGroup settingsPanel;

    [Header("Main")]
    public RectTransform titleRect;
    public RectTransform buttonsRect;
    public TextMeshProUGUI highscoreText;

    [Header("Settings")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Background")]
    public RectTransform[] bgParticles; // opcjonalne kwadraty w tle

    void Start()
    {
        settingsPanel.alpha = 0f;
        settingsPanel.interactable = false;
        settingsPanel.blocksRaycasts = false;

        mainPanel.alpha = 0f;

        highscoreText.text = "Best: " + HighscoreSystem.Get();

        // odepnij listenery przed ustawieniem wartoœci
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        // ustaw wartoœci bez triggerowania eventów
        if (musicSlider) musicSlider.SetValueWithoutNotify(AudioManager.Instance ? AudioManager.Instance.MusicVolume : 0.5f);
        if (sfxSlider) sfxSlider.SetValueWithoutNotify(AudioManager.Instance ? AudioManager.Instance.SfxVolume : 1f);

        // podepnij listenery
        musicSlider.onValueChanged.AddListener(OnMusicSlider);
        sfxSlider.onValueChanged.AddListener(OnSfxSlider);

        // animacje wejœcia
        MenuAnimator.Instance.FadeIn(mainPanel, 0.5f);
        MenuAnimator.Instance.SlideIn(titleRect, 0.6f);
        MenuAnimator.Instance.SlideIn(buttonsRect, 0.7f);
    }

    public void StartGame()
    {
        AudioManager.Instance?.PlayClick();
        MenuAnimator.Instance.FadeOut(mainPanel, 0.3f, () =>
        {
            SceneManager.LoadScene("Game");
        });
    }

    public void OpenSettings()
    {
        AudioManager.Instance?.PlayClick();
        MenuAnimator.Instance.FadeOut(mainPanel, 0.2f);
        MenuAnimator.Instance.FadeIn(settingsPanel, 0.2f);
        settingsPanel.interactable = true;
        settingsPanel.blocksRaycasts = true;
    }

    public void CloseSettings()
    {
        AudioManager.Instance?.PlayClick();
        MenuAnimator.Instance.FadeOut(settingsPanel, 0.2f);
        MenuAnimator.Instance.FadeIn(mainPanel, 0.2f);
        settingsPanel.interactable = false;
        settingsPanel.blocksRaycasts = false;
    }

    public void OnMusicSlider(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    public void OnSfxSlider(float value)
    {
        AudioManager.Instance?.SetSfxVolume(value);
    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlayClick();
        Application.Quit();
    }

    // hover efekt na przyciskach
    public void OnButtonHover(RectTransform btn)
    {
        MenuAnimator.Instance.Pulse(btn);
        AudioManager.Instance?.PlayClick();
    }
}