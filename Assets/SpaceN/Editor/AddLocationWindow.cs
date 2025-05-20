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
            AddLocationWindow window = GetWindow<AddLocationWindow>(true, "�������� ����� �������", true);
            window.selectedType = type;
            onAddCallback = callback;
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("���������� ����� �������", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("���: " + selectedType, EditorStyles.label);

            GUILayout.BeginHorizontal();
            GUILayout.Label("����:", GUILayout.Width(50));
            if (GUILayout.Button(selectedLanguage.ToUpper(), GUILayout.Width(50)))
            {
                selectedLanguage = selectedLanguage == "ru" ? "en" : "ru";
            }
            GUILayout.EndHorizontal();

            tag = EditorGUILayout.TextField("���:", tag);
            localizedText = EditorGUILayout.TextField("��������:", localizedText);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("��������"))
            {
                if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(localizedText))
                {
                    onAddCallback?.Invoke(selectedType, tag, localizedText);
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("������", "������� � ���, � ��������!", "��");
                }
            }
            if (GUILayout.Button("������")) Close();
            GUILayout.EndHorizontal();
        }
    }
}
