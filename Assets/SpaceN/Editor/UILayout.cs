using UnityEditor;
using UnityEngine;

namespace SpaceN.Editor
{
    public static class UILayout
    {
        // Размеры и отступы ноды
        public static readonly float NodeWidth = 200f;
        public static readonly float NodeHeight = 100f;
        public static readonly float NodePadding = 10f;

        // Панель
        public static readonly float ToolbarHeight = 36f;
        public static readonly float ButtomHeight = 26f;
        public static readonly float RightPanelWidth = 350f;
        public static readonly float RightPanelMinWidth = 50f;
        public static readonly float RightPanelFraction = 2f / 3f;
        public static readonly float RightPanelResizeAreaWidth = 5f;

        // Сетка
        public static readonly float GridSpacing = 20f;
        public static readonly float GridLineThickness = 0.2f;

        // Цвета
        public static readonly Color GridColor = new Color(0.5f, 0.5f, 0.5f, 0.2f); //new Color(125, 125, 125, 60) | Unity: (0.5f, 0.5f, 0.5f, 0.2f);
        public static readonly Color NodeOutlineColor = new Color(0f, 1f, 0f, 5f); //new Color(0, 255, 0, 230); | Unity: (0f, 1f, 0f, 5f);
        public static readonly Color RightPanelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f); //new Color(25, 25, 25, 216); | Unity: (0.1f, 0.1f, 0.1f, 0.85f);

        // Стили
        public static GUIStyle ToolbarButtonStyle
        {
            get
            {
                if (_toolbarButtonStyle == null)
                {
                    _toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        fixedHeight = ButtomHeight,
                        padding = new RectOffset(5, 5, 5, 5),
                        margin = new RectOffset(5, 5, 5, 5)
                    };
                }
                return _toolbarButtonStyle;
            }
        }
        private static GUIStyle _toolbarButtonStyle;

        public static GUIStyle ToolbarStyle
        {
            get
            {
                if (_toolbarStyle == null)
                {
                    _toolbarStyle = new GUIStyle(EditorStyles.toolbar)
                    {
                        fixedHeight = ToolbarHeight,
                        padding = new RectOffset(2, 2, 2, 2), // Уменьшаем отступы
                        margin = new RectOffset(2, 2, 2, 2), // Уменьшаем внешние отступы
                        normal = { background = EditorGUIUtils.CreateColorTexture(Color.black) }
                    };
                }
                return _toolbarStyle;
            }
        }
        private static GUIStyle _toolbarStyle;

        public static GUIStyle BoldLabelStyle
        {
            get
            {
                if (_boldLabelStyle == null)
                {
                    _boldLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 14,
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return _boldLabelStyle;
            }
        }
        private static GUIStyle _boldLabelStyle;
    }
}