using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace SpaceN.Editor
{
    public class EventProcessor
    {
        private NodeEditorWindow window;

        public EventProcessor(NodeEditorWindow window)
        {
            this.window = window;
        }

        public void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1) // ��� � �������� ����������� ����
                    {
                        window.ShowContextMenu(e.mousePosition);
                        e.Use();
                    }
                    else if (e.button == 0) // ���
                    {
                        // ���� � ������� ������� (�� � ������ ������)
                        if (e.mousePosition.x < window.position.width - window.panelWidth)
                        {
                            Node clickedNode = window.GetNodeAtPoint(e.mousePosition);
                            if (clickedNode != null)
                            {
                                // ���� �������� �� ������ ���� - ������� �����
                                if (window.activeNode != clickedNode)
                                {
                                    window.ClearFocus();
                                }

                                window.activeNode = clickedNode;
                                window.isDraggingNode = true;
                                //GUI.changed = true;
                            }
                            else
                            {
                                // ���� �� ������� ����� - ������� �����
                                window.ClearFocus();
                                window.isDraggingNode = false;
                            }
                            window.mouseDownPosition = e.mousePosition;
                            window.isDragging = false;
                            GUI.changed = true;
                        }
                        else // ���� � ������ ������
                        {
                            // ���������, ��� �� ���� �� �������� ����������
                            //bool clickedOnControl = false;
                            //if (window.activeNode != null)
                            //{
                                // ����� ����� �������� �������� ������� ����� ������������ ��������� UI
                                // ��������, ���� ���� ������ ��� ������ ������������� ��������
                            //}

                            //if (!clickedOnControl)
                            //{
                            //    window.ClearFocus();
                            //}
                            e.Use();
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        // ���� ������ � ������� ������� � �� ���������� ��������� ������� ������
                        if (e.mousePosition.x < window.position.width - window.panelWidth && !window.isResizingPanel)
                        {
                            // ���� ���������� �� ��������� ����� ��������� �����, �������� drag
                            if (!window.isDragging && Vector2.Distance(e.mousePosition, window.mouseDownPosition) > window.clickThreshold)
                            {
                                window.isDragging = true;
                            }
                            if (window.isDragging)
                            {
                                if (window.isDraggingNode)
                                {
                                    // ���������� ������ �������� ����
                                    window.activeNode.Drag(e.delta);
                                }
                                else
                                {
                                    // ���������� ������� �������
                                    window.OnDrag(e.delta);
                                }
                            }
                        }
                    }
                    break;

                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        // ���� �� ���� �������������� (�� ���� ��� ��������� ����) � ���� � ������� �������
                        if (!window.isDragging && e.mousePosition.x < window.position.width - window.panelWidth)
                        {
                            // ���� ��� �������� ������ ��� ����, ���������� activeNode
                            Node clickedNode = window.GetNodeAtPoint(e.mousePosition);
                            if (clickedNode == null)
                            {
                                window.activeNode = null;
                                UnityEngine.Debug.Log("�������� ���� �������� (��������� ���� �� ������ �����).");
                                GUI.changed = true;
                            }
                        }
                        window.isDragging = false; // ���������� ���� ����������
                        window.isDraggingNode = false; // ���������� ���� �������������� ����
                        //e.Use();
                    }
                    break;
            }
        }

        public void ProcessNodeEvents(Event e)
        {
            if (window.nodes != null)
            {
                for (int i = window.nodes.Count - 1; i >= 0; i--)
                {
                    bool guiChanged = window.nodes[i].ProcessEvents(e);
                    if (guiChanged)
                        GUI.changed = true;
                }
            }
        }

        public void ProcessPanelResize(Event e, Rect resizeRect)
        {
            // ���� ������ ���� �� ������� ����, ���������� ��������� ��������
            if (!window.position.Contains(e.mousePosition))
            {
                window.isResizingPanel = false;
                return;
            }

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (resizeRect.Contains(e.mousePosition))
                    {
                        window.isResizingPanel = true;
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (window.isResizingPanel)
                    {
                        // ����� ������ ������ ����������� ��� ���������� �� ������ ������� ���� �� ����
                        window.panelWidth = window.position.width - e.mousePosition.x;
                        // ������������ panelWidth �� minPanelWidth �� 2/3 ������ ����
                        window.panelWidth = Mathf.Clamp(window.panelWidth, window.minPanelWidth, window.position.width * window.maxPanelWidthFraction);
                        GUI.changed = true;
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    window.isResizingPanel = false;
                    break;
            }
        }
    }
}