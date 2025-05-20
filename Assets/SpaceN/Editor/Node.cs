using System.Collections.Generic;
using SpaceN.Scripts;
using UnityEngine;

namespace SpaceN.Editor
{
    public class Node
    {
        public Rect rect;
        public string id;
        public string title;
        public string description;
        public bool isExpanded = false;
        public bool isDragged;
        public bool toBeDeleted = false;
        public List<string> connectedNodeIds = new List<string>();
        public EventData eventData;
        public LocationData locationData;
        public string imageName;
        public Texture2D nodeImage;

        public Node(Vector2 position, float width, float height, string id, string title, string description)
        {
            rect = new Rect(position.x, position.y, width, height);
            this.id = id;
            this.title = title;
            this.description = description;
            eventData = null;
        }

        public Node(Vector2 position, float width, float height, EventData eventData, GameData data)
        {
            rect = new Rect(position.x, position.y, width, height);
            this.eventData = eventData ?? new EventData();
            id = this.eventData.id;
            imageName = this.eventData.image;
            description = this.eventData.description != null ? eventData.description: "";

            // Инициализация location, если ее нет
            if (this.eventData.location == null)
            {
                this.eventData.location = new LocationData();
            }

            if (!string.IsNullOrEmpty(eventData.image))
            {
                string resourcePath = $"Images/Locations/{eventData.image}";
                nodeImage = Resources.Load<Texture2D>(resourcePath);
                if (nodeImage == null)
                    Debug.LogWarning($"Изображение не найдено: {resourcePath}");
            }
        }

        public Vector2 Center => new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);

        public Vector2 GetConnectionPoint(Vector2 targetPoint)
        {
            Vector2 center = Center;
            Vector2 diff = targetPoint - center;

            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                float x = diff.x > 0 ? rect.xMax : rect.x;
                return new Vector2(x, center.y);
            }
            else
            {
                float y = diff.y > 0 ? rect.yMax : rect.y;
                return new Vector2(center.x, y);
            }
        }

        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        public bool ProcessEvents(Event e)
        {
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                if (e.button == 0)
                {
                    isDragged = true;
                    return true;
                }
            }
            if (e.type == EventType.MouseUp)
            {
                isDragged = false;
            }
            if (e.type == EventType.MouseDrag && isDragged)
            {
                Drag(e.delta);
                return true;
            }
            return false;
        }

        public Vector2 GetOutputPosition() => new Vector2(rect.x + rect.width / 2, rect.y + rect.height);
        public Vector2 GetInputPosition() => new Vector2(rect.x + rect.width / 2, rect.y);
    }
}