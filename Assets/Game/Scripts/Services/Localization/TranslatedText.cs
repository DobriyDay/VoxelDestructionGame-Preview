using System;
using UnityEngine;
using UnityEngine.UI;

public class TranslatedText : MonoBehaviour
{
    [SerializeField] private bool automaticallyUpdate = true;
    [SerializeField] private string localizationKey;
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    public string GetTranslation(string key = null)
    {
        if (text == null)
            text = GetComponent<Text>();
        
        key ??= localizationKey;

        string translatedText = LocalizationManager.GetText(key);
        return translatedText;
    }

    public void Translate(string key = null)
    {
        if (text == null)
            text = GetComponent<Text>();
        text.text = GetTranslation(key);
    }

    private void OnEnable()
    {
        if (automaticallyUpdate)
            Translate();
    }
}