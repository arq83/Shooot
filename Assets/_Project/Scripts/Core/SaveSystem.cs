using UnityEngine;

public static class SaveSystem
{
    private static string key = "SAVE_DATA";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    public static SaveData Load()
    {
        if (!PlayerPrefs.HasKey(key))
            return null;

        string json = PlayerPrefs.GetString(key);
        return JsonUtility.FromJson<SaveData>(json);
    }
}