using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager
{
    [System.Serializable]
    public class LocalizationFile
    {
        public LocalizationEntry[] entries;
    }

    [System.Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string en;
        public string ru;
    }

    
    private static Dictionary<string, Dictionary<string, string>> _localization = new();
    private static string _currentLanguage = "en";
    private const string DEFAULT_LANGUAGE = "en";

    public static void Init(TextAsset file, string language)
    {
        _currentLanguage = language;

        LocalizationFile data = JsonUtility.FromJson<LocalizationFile>(file.text);
        _localization.Clear();

        foreach (var entry in data.entries)
        {
            var dict = new Dictionary<string, string>
            {
                { "en", entry.en },
                { "ru", entry.ru }
            };

            _localization[entry.key] = dict;
        }
    }

    public static string GetText(string key)
    {
        if (_localization.TryGetValue(key, out var translations))
        {
            if (translations.TryGetValue(_currentLanguage, out var value))
                return value;

            if (translations.TryGetValue(DEFAULT_LANGUAGE, out var fallback))
                return fallback;
        }

        Debug.LogWarning($"Missing translation for key: {key}");
        return key;
    }
}