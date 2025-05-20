// EditorLocalizationManager.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using Newtonsoft.Json;
using SpaceN.Scripts;
using UnityEditor;
using UnityEngine;

namespace SpaceN.Editor
{
    public class EditorLocalizationManager : ScriptableObject
    {
        public static EditorLocalizationManager Instance { get; private set; }
    
        public static List<string> Languages = new List<string>(LocalizationLanguages.GetLanguageNames());

        public static string CurrentLanguage = LocalizationLanguages.CurrentLanguageName;

        [SerializeField]
        private Dictionary<string, string> _localizationData = new Dictionary<string, string>();

        private const string LocalizationFileName = "Event_Localization";
        private const string ResourcesRoot = "Assets/SpaceN/Resources/";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (Instance == null)
            {
                Instance = CreateInstance<EditorLocalizationManager>();
                //Instance.LoadAllLocalizations();
                Instance.LoadLocalizationForLanguage(CurrentLanguage); // Загружаем локализацию для текущего языка
            }
        }

        public void RenameLocalizationKey(string oldKey, string newKey, string value)
        {
            if (string.IsNullOrEmpty(oldKey)) return;

            if (_localizationData.ContainsKey(oldKey))
            {
                string text = _localizationData[oldKey];
                _localizationData.Remove(oldKey);

                if (!string.IsNullOrEmpty(newKey))
                {
                    _localizationData[newKey] = text;
                }
            }

            if (!string.IsNullOrEmpty(newKey) && !string.IsNullOrEmpty(value))
            {
                _localizationData[newKey] = value;
            }
        }

        public void LoadLocalizationForLanguage(string languageCode)
        {
            string path = $"{ResourcesRoot}Localizations/{LocalizationFileName}_{languageCode}.json";
            TextAsset jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (jsonFile == null)
            {
                Debug.LogError($"Локализация для языка {languageCode} не найдена!");
                _localizationData = new Dictionary<string, string>();
                return;
            }

            // Десериализуем JSON в словарь с помощью Newtonsoft.Json
            try
            {
                _localizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFile.text) ?? new Dictionary<string, string>();
                Debug.Log($"Локализация для языка {languageCode} успешно загружена. Загружено ключей: {_localizationData.Count}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка загрузки локализации: {e.Message}");
                _localizationData = new Dictionary<string, string>();
            }
        }
    
        public string GetLocalizedText(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "NULL_KEY";

            if (_localizationData == null)
                _localizationData = new Dictionary<string, string>();

            if (_localizationData.TryGetValue(key, out string value))
            {
                //string localizedText = localizationData[key];
                return value?.Replace(@"\n", "\n")?.Replace(@"\t", "    ") ?? $"MISSING: {key}";
                // Заменяем \n на новую строку и \t на несколько пробелов для имитации табуляции
                //localizedText = localizedText.Replace(@"\n", "\n");
                //localizedText = localizedText.Replace(@"\t", "    "); // Заменяем табуляцию на 4 пробела
                //return localizedText;
            }
            return $"MISSING: {key}"; // Если перевода нет, возвращаем ключ с пометкой
        }

        public void UpdateLocalization(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (_localizationData == null)
                _localizationData = new Dictionary<string, string>();

            _localizationData[key] = value;
        }

        public void RemoveLocalizationKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (_localizationData != null && _localizationData.ContainsKey(key))
            {
                _localizationData.Remove(key);
            }
        }

        public void SaveLocalizationToFile(string languageCode)
        {
            if (_localizationData == null)
                _localizationData = new Dictionary<string, string>();

            string path = $"{ResourcesRoot}Localizations/{LocalizationFileName}_{languageCode}.json";
            string json = JsonConvert.SerializeObject(_localizationData, Formatting.Indented);

            try
            {
                System.IO.File.WriteAllText(path, json);
                AssetDatabase.Refresh();
                Debug.Log($"Локализация сохранена: {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка сохранения локализации: {e.Message}");
            }
        }

    }
}
#endif