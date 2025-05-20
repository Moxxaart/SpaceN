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
        private Dictionary<string, string> localizationData; // ������ ������ ������ ����� � �������� ��� �������� �����    
        public string currentLanguage = "en"; // ���� �� ���������

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // ����� ����� ��������� ����������� ��� ��������� �������:
                //LoadLocalizationFromFolder("Localizations");
                LoadLocalizationForLanguage(currentLanguage); // ��������� ����������� ��� �������� �����

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
                Debug.LogError($"����������� ��� ����� {languageCode} �� �������!");
                return;
            }

            // ������������� JSON � ������� � ������� Newtonsoft.Json
            localizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFile.text);

            if (localizationData == null)
            {
                Debug.LogError($"������ ��� �������� ����������� ��� ����� {languageCode}!");
                return;
            }

            Debug.Log($"����������� ��� ����� {languageCode} ������� ���������. ��������� ������: {localizationData.Count}");
        }

        public string GetLocalizedText(string key)
        {
            if (localizationData != null && localizationData.ContainsKey(key))
            {
                string localizedText = localizationData[key];

                // �������� \n �� ����� ������ � \t �� ��������� �������� ��� �������� ���������
                localizedText = localizedText.Replace(@"\n", "\n");
                localizedText = localizedText.Replace(@"\t", "    "); // �������� ��������� �� 4 �������

                return localizedText;
            }
            return key; // ���� �������� ���, ���������� ��� ����
        }
    }
}
