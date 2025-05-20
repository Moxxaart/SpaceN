using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace SpaceN.Scripts
{
    public class LocalizationManager : MonoBehaviour
    {
        private const string LocalizationFileName = "Event_Localization";
        private const string ResourcesRoot = "Assets/SpaceN/Resources/";
        
        public static LocalizationManager Instance { get; private set; }
        //private Dictionary<string, Dictionary<string, string>> localizationData;
        private Dictionary<string, string> localizationData; // Теперь храним только ключи и значения для текущего языка    
        public string currentLanguage = "en"; // Язык по умолчанию

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // Можно сразу загрузить локализацию для стартовой локации:
                //LoadLocalizationFromFolder("Localizations");
                LoadLocalizationForLanguage(currentLanguage); // Загружаем локализацию для текущего языка

            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadLocalizationForLanguage(string languageCode)
        {
            string path = $"{ResourcesRoot}Localizations/{LocalizationFileName}_{languageCode}.json";
            TextAsset jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (jsonFile == null)
            {
                Debug.LogError($"Локализация для языка {languageCode} не найдена!");
                return;
            }

            // Десериализуем JSON в словарь с помощью Newtonsoft.Json
            localizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFile.text);

            if (localizationData == null)
            {
                Debug.LogError($"Ошибка при загрузке локализации для языка {languageCode}!");
                return;
            }

            Debug.Log($"Локализация для языка {languageCode} успешно загружена. Загружено ключей: {localizationData.Count}");
        }

        public string GetLocalizedText(string key)
        {
            if (localizationData != null && localizationData.ContainsKey(key))
            {
                string localizedText = localizationData[key];

                // Заменяем \n на новую строку и \t на несколько пробелов для имитации табуляции
                localizedText = localizedText.Replace(@"\n", "\n");
                localizedText = localizedText.Replace(@"\t", "    "); // Заменяем табуляцию на 4 пробела

                return localizedText;
            }
            return key; // Если перевода нет, возвращаем сам ключ
        }
    }
}
