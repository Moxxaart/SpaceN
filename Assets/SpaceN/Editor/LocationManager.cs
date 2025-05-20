using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

namespace SpaceN.Editor
{
    public static class LocationManager
    {
        private static List<string> globalLocations;
        private static List<string> locations;
        private static List<string> sublocations;
        private static bool isLoaded = false;

        private static string currentLanguage = "ru"; // По умолчанию русский язык

        private const string folder = "Location_category";
        private const string fileName = "locations";
        //private string fileNameLocalization = "locations_locale_ru";

        public static void LoadLocationData()
        {
            if (isLoaded) return; // Предотвращаем повторную загрузку

            TextAsset jsonFile = Resources.Load<TextAsset>($"{folder}/{fileName}");
            if (jsonFile != null)
            {
                LocationData data = JsonUtility.FromJson<LocationData>(jsonFile.text);
                globalLocations = data.global_locations;
                locations = data.locations;
                sublocations = data.sublocations;
                isLoaded = true;
            }
            else
            {
                Debug.LogError("Файл locations.json не найден!");
            }
        }

        public static Dictionary<string, string> LoadLocalizations(string lang)
        {
            Dictionary<string, string> localizations = new Dictionary<string, string>();
            string fileNameLocalization = $"locations_locale_{lang}";
            TextAsset jsonFile = Resources.Load<TextAsset>($"{folder}/{fileNameLocalization}");
            if (jsonFile != null)
            {
                //Debug.Log($"Файл локализации {fileNameLocalization}.json успешно загружен.");
                localizations = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFile.text);
                //Debug.Log($"Загружено {localizations.Count} локализаций.");
            }
            else
            {
                //Debug.LogError($"Файл локализации {fileNameLocalization}.json не найден!");
            }
            return localizations;
        }

        public static void SetCurrentLanguage(string lang)
        {
            currentLanguage = lang;
        }

        public static string GetCurrentLanguage()
        {
            return currentLanguage;
        }

        public static string GetLocalizedGlobalLocation(string tag)
        {
            var localizations = LoadLocalizations(currentLanguage);
            return localizations.TryGetValue(tag, out string localized) ? localized : tag;
        }

        public static string GetLocalizedLocation(string tag)
        {
            var localizations = LoadLocalizations(currentLanguage);
            return localizations.TryGetValue(tag, out string localized) ? localized : tag;
        }

        public static string GetLocalizedSublocation(string tag)
        {
            var localizations = LoadLocalizations(currentLanguage);
            return localizations.TryGetValue(tag, out string localized) ? localized : tag;
        }

        public static List<string> GetGlobalLocations() => globalLocations ?? new List<string>();
        public static List<string> GetLocations() => locations ?? new List<string>();
        public static List<string> GetSublocations() => sublocations ?? new List<string>();


        [System.Serializable]
        private class LocationData
        {
            public List<string> global_locations;
            public List<string> locations;
            public List<string> sublocations;
        }
    }
}
