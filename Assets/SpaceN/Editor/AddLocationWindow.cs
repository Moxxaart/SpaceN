using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace SpaceN.Editor
{
    public class AddLocationWindow : EditorWindow
    {
        private string tag = "";
        private string localizedText = "";
        private string selectedType = "Global";
        private string selectedLanguage = "ru";
        private static System.Action<string, string, string> onAddCallback;

        public static void ShowWindow(string type, System.Action<string, string, string> callback)
        {
            AddLocationWindow window = GetWindow<AddLocationWindow>(true, "Добавить новую локацию", true);
            window.selectedType = type;
            onAddCallback = callback;
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Добавление новой локации", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Тип: " + selectedType, EditorStyles.label);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Язык:", GUILayout.Width(50));
            if (GUILayout.Button(selectedLanguage.ToUpper(), GUILayout.Width(50)))
            {
                selectedLanguage = selectedLanguage == "ru" ? "en" : "ru";
            }
            GUILayout.EndHorizontal();

            tag = EditorGUILayout.TextField("Тег:", tag);
            localizedText = EditorGUILayout.TextField("Название:", localizedText);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Добавить"))
            {
                if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(localizedText))
                {
                    onAddCallback?.Invoke(selectedType, tag, localizedText);
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Ошибка", "Введите и тег, и название!", "Ок");
                }
            }
            if (GUILayout.Button("Отмена")) Close();
            GUILayout.EndHorizontal();
        }
    }
}
