using System.Collections.Generic;
using System.Linq;
using SpaceN.Editor;
using UnityEditor;
using UnityEngine;

public static class EditorGUIUtils
{
    public static Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
    
    public static Vector2 CalculateNodePosition(int index, int columns = 3, int xSpacing = 250, int ySpacing = 150)
    {
        int row = index / columns;
        int col = index % columns;
        return new Vector2(col * xSpacing, row * ySpacing);
    }
    
    public static void DrawConnections(List<Node> nodes)
    {
        if (nodes.Count == 0) return;

        // Построим словарь для быстрого поиска по id
        var nodeMap = nodes.ToDictionary(n => n.id);
        Handles.BeginGUI();

        foreach (Node node in nodes)
        {
            foreach (string targetId in node.connectedNodeIds)
            {
                // Чтобы не рисовать пару дважды, рисуем только если node.id < targetId
                if (string.Compare(node.id, targetId) >= 0)
                    continue;

                if (!nodeMap.TryGetValue(targetId, out Node targetNode))
                    continue;

                // Определяем, является ли связь двусторонней:
                bool bidirectional = targetNode.connectedNodeIds.Contains(node.id);

                // Вычисляем точки подключения на границе нод, используя их метод GetConnectionPoint
                Vector2 startPoint = node.GetConnectionPoint(targetNode.Center);
                Vector2 endPoint = targetNode.GetConnectionPoint(node.Center);

                if (bidirectional)
                {
                    // Для двусторонней связи рисуем двойную линию.
                    // Вычисляем единичный вектор направления от startPoint к endPoint
                    Vector2 direction = (endPoint - startPoint).normalized;
                    // Перпендикуляр к линии
                    Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                    float offsetDistance = 4f; // смещение для параллельных линий
                    Vector2 offsetVector = perpendicular * offsetDistance;

                    // Вычисляем контрольные точки для кривых
                    Vector2 controlOffset = (Mathf.Abs(startPoint.x - endPoint.x) > Mathf.Abs(startPoint.y - endPoint.y)) 
                        ? new Vector2(50, 0) : new Vector2(0, 50);

                    // Первая параллельная кривая (смещаем точки на +offsetVector)
                    Vector2 startTangent1 = startPoint + offsetVector + controlOffset;
                    Vector2 endTangent1 = endPoint + offsetVector - controlOffset;
                    Handles.DrawBezier(startPoint + offsetVector, endPoint + offsetVector, startTangent1, endTangent1, Color.cyan, null, 3f);

                    // Вторая параллельная кривая (смещаем точки на -offsetVector)
                    Vector2 startTangent2 = startPoint - offsetVector + controlOffset;
                    Vector2 endTangent2 = endPoint - offsetVector - controlOffset;
                    Handles.DrawBezier(startPoint - offsetVector, endPoint - offsetVector, startTangent2, endTangent2, Color.cyan, null, 3f);
                }
                else
                {
                    // Односторонняя связь – рисуем обычную кривую
                    Vector2 controlOffset = (Mathf.Abs(startPoint.x - endPoint.x) > Mathf.Abs(startPoint.y - endPoint.y)) 
                        ? new Vector2(50, 0) : new Vector2(0, 50);
                    Vector2 startTangent = startPoint + controlOffset;
                    Vector2 endTangent = endPoint - controlOffset;
                    Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, Color.cyan, null, 3f);
                }
            }
        }
        Handles.EndGUI();
    }
    
    public static void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor, Vector2 offset, Vector2 drag, Rect position)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }
        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }
}