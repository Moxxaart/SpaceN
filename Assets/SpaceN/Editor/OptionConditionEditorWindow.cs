using SpaceN.Scripts;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpaceN.Editor
{
    public class OptionConditionEditorWindow : EditorWindow
    {
        private OptionData editingOption;
        private Action<OptionData> onSaveCallback;

        private Vector2 conditionsScroll;
        private Vector2 effectsScroll;

        private string newConditionType = "HasAtLeast";
        private string newEffectType = "IncreaseParameter";
        private string newParameterName = "Money";
        private int newValue = 100;

        public static void Open(OptionData option, Action<OptionData> onSave)
        {
            var window = GetWindow<OptionConditionEditorWindow>(true, "Option Conditions Editor");
            window.editingOption = option;
            window.onSaveCallback = onSave;
            window.minSize = new Vector2(500, 300);
        }

        void OnGUI()
        {
            if (editingOption == null) return;

            GUILayout.Space(10);

            // CONDITIONS SECTION
            EditorGUILayout.LabelField("CONDITIONS", EditorStyles.boldLabel);
            DrawConditionsSection();

            GUILayout.Space(20);

            // EFFECTS SECTION
            EditorGUILayout.LabelField("EFFECTS", EditorStyles.boldLabel);
            DrawEffectsSection();

            GUILayout.FlexibleSpace();
            DrawActionButtons();
        }

        void DrawConditionsSection()
        {
            conditionsScroll = EditorGUILayout.BeginScrollView(conditionsScroll, GUILayout.Height(100));

            if (editingOption.conditions == null || editingOption.conditions.Count == 0)
            {
                EditorGUILayout.HelpBox("No conditions set", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < editingOption.conditions.Count; i++)
                {
                    var cond = editingOption.conditions[i];
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(cond.type, GUILayout.Width(120));
                    EditorGUILayout.LabelField(cond.parameter, GUILayout.Width(80));
                    EditorGUILayout.LabelField(cond.value.ToString(), GUILayout.Width(50));

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        editingOption.conditions.RemoveAt(i);
                        i--;
                        GUI.changed = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            // ADD NEW CONDITION
            EditorGUILayout.BeginHorizontal();
            newConditionType = EditorGUILayout.TextField(newConditionType, GUILayout.Width(120));
            newParameterName = EditorGUILayout.TextField(newParameterName, GUILayout.Width(80));
            newValue = EditorGUILayout.IntField(newValue, GUILayout.Width(50));

            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                if (editingOption.conditions == null)
                    editingOption.conditions = new List<ConditionData>();

                editingOption.conditions.Add(new ConditionData
                {
                    type = newConditionType,
                    parameter = newParameterName,
                    value = newValue
                });
                GUI.changed = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawEffectsSection()
        {
            effectsScroll = EditorGUILayout.BeginScrollView(effectsScroll, GUILayout.Height(100));

            if (editingOption.effects == null || editingOption.effects.Count == 0)
            {
                EditorGUILayout.HelpBox("No effects set", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < editingOption.effects.Count; i++)
                {
                    var effect = editingOption.effects[i];
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(effect.type, GUILayout.Width(120));
                    EditorGUILayout.LabelField(effect.parameter, GUILayout.Width(80));
                    EditorGUILayout.LabelField(effect.value.ToString(), GUILayout.Width(50));

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        editingOption.effects.RemoveAt(i);
                        i--;
                        GUI.changed = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            // ADD NEW EFFECT
            EditorGUILayout.BeginHorizontal();
            newEffectType = EditorGUILayout.TextField(newEffectType, GUILayout.Width(120));
            newParameterName = EditorGUILayout.TextField(newParameterName, GUILayout.Width(80));
            newValue = EditorGUILayout.IntField(newValue, GUILayout.Width(50));

            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                if (editingOption.effects == null)
                    editingOption.effects = new List<EventEffect>();

                editingOption.effects.Add(new EventEffect
                {
                    type = newEffectType,
                    parameter = newParameterName,
                    value = newValue
                });
                GUI.changed = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawActionButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                onSaveCallback?.Invoke(editingOption);
                Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}