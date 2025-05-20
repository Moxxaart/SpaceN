using UnityEngine;

namespace SpaceN.Editor
{
    public class NodeRenderer
    {
        private Node _node;

        public NodeRenderer(Node node)
        {
            _node = node;
        }

        public void Draw()
        {
            Color prevColor = GUI.color;
            Rect currentRect = _node.rect;

            string localizedTitle = EditorLocalizationManager.Instance.GetLocalizedText(_node.id);
            string localizedDesc = EditorLocalizationManager.Instance.GetLocalizedText(_node.description);

            if (!_node.isExpanded)
            {
                DrawCollapsedNode(currentRect, localizedTitle);
                _node.rect.width = 150;
                _node.rect.height = 75;
            }
            else
            {
                _node.rect.width = 400;
                _node.rect.height = 200;
                DrawExpandedNode(currentRect, localizedTitle, localizedDesc);
            }

            DrawButtons(currentRect);
            GUI.color = prevColor;
        }

        private void DrawCollapsedNode(Rect currentRect, string localizedTitle)
        {
            GUI.color = Color.white;
            GUI.backgroundColor = Color.black;
            GUI.Box(currentRect, localizedTitle);
        }

        private void DrawExpandedNode(Rect currentRect, string localizedTitle, string localizedDesc)
        {
            GUI.color = new Color(1, 1, 1, 2.4f);
            GUI.backgroundColor = Color.black;
            GUI.Box(currentRect, string.Empty);

            Rect idRect = new Rect(currentRect.x + 60, currentRect.y + 5, currentRect.width - 65, 20);
            GUI.Label(idRect, "ID: " + _node.id);
            Rect titleRect = new Rect(currentRect.x + 60, currentRect.y + 30, currentRect.width - 65, 20);
            GUI.Label(titleRect, "Title: " + localizedTitle);

            Rect descRect = new Rect(currentRect.x + 5, currentRect.y + 60, currentRect.width - 10, 40);
            GUI.Label(descRect, "Desc: " + localizedDesc);

            if (_node.nodeImage != null)
            {
                Rect imageRect = new Rect(currentRect.x + 5, currentRect.y + 5, 50, 50);
                GUI.DrawTexture(imageRect, _node.nodeImage, ScaleMode.ScaleToFit);
            }
        }

        private void DrawButtons(Rect currentRect)
        {
            Rect toggleRect = new Rect(currentRect.x, currentRect.y, 20, 20);
            if (GUI.Button(toggleRect, _node.isExpanded ? "–" : "+"))
            {
                _node.isExpanded = !_node.isExpanded;
            }

            Rect deleteRect = new Rect(currentRect.xMax - 20, currentRect.y, 20, 20);
            Color prevBgColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUI.Button(deleteRect, "X"))
            {
                _node.toBeDeleted = true;
            }
            GUI.backgroundColor = prevBgColor;
        }
    }
}