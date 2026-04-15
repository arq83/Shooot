public static class HighscoreSystem
{
    private const string Key = "HIGHSCORE";

    public static int Get() => UnityEngine.PlayerPrefs.GetInt(Key, 0);

    public static void Submit(int score)
    {
        if (score > Get())
            UnityEngine.PlayerPrefs.SetInt(Key, score);
    }
}