using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LoadingText : MonoBehaviour
{
        [System.Serializable]
        public class TranslatedText
        {
                public string language;
                public Sprite textSprite;
        }

        [SerializeField] private TranslatedText[] translatedTexts;
        [SerializeField] private Image textImage;
        
        private void OnEnable()
        {
                textImage.sprite = translatedTexts.First(t => t.language == Global.PlatformSDK.Language).textSprite;
        }
}