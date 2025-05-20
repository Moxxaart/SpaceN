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
                    if (e.button == 1) // ПКМ – вызываем контекстное меню
                    {
                        window.ShowContextMenu(e.mousePosition);
                        e.Use();
                    }
                    else if (e.button == 0) // ЛКМ
                    {
                        // Клик в рабочей области (не в правой панели)
                        if (e.mousePosition.x < window.position.width - window.panelWidth)
                        {
                            Node clickedNode = window.GetNodeAtPoint(e.mousePosition);
                            if (clickedNode != null)
                            {
                                // Если кликнули по другой ноде - снимаем фокус
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
                                // Клик по пустому месту - снимаем фокус
                                window.ClearFocus();
                                window.isDraggingNode = false;
                            }
                            window.mouseDownPosition = e.mousePosition;
                            window.isDragging = false;
                            GUI.changed = true;
                        }
                        else // Клик в правой панели
                        {
                            // Проверяем, был ли клик на элементе управления
                            //bool clickedOnControl = false;
                            //if (window.activeNode != null)
                            //{
                                // Здесь можно добавить проверку позиции клика относительно элементов UI
                                // Например, если есть кнопки или другие интерактивные элементы
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
                        // Если курсор в рабочей области и не происходит изменение размера панели
                        if (e.mousePosition.x < window.position.width - window.panelWidth && !window.isResizingPanel)
                        {
                            // Если расстояние от начальной точки превышает порог, начинаем drag
                            if (!window.isDragging && Vector2.Distance(e.mousePosition, window.mouseDownPosition) > window.clickThreshold)
                            {
                                window.isDragging = true;
                            }
                            if (window.isDragging)
                            {
                                if (window.isDraggingNode)
                                {
                                    // Перемещаем только активную ноду
                                    window.activeNode.Drag(e.delta);
                                }
                                else
                                {
                                    // Перемещаем рабочую область
                                    window.OnDrag(e.delta);
                                }
                            }
                        }
                    }
                    break;

                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        // Если не было перетаскивания (то есть это одиночный клик) и клик в рабочей области
                        if (!window.isDragging && e.mousePosition.x < window.position.width - window.panelWidth)
                        {
                            // Если под курсором теперь нет ноды, сбрасываем activeNode
                            Node clickedNode = window.GetNodeAtPoint(e.mousePosition);
                            if (clickedNode == null)
                            {
                                window.activeNode = null;
                                UnityEngine.Debug.Log("Активная нода сброшена (одиночный клик на пустом месте).");
                                GUI.changed = true;
                            }
                        }
                        window.isDragging = false; // Сбрасываем флаг независимо
                        window.isDraggingNode = false; // Сбрасываем флаг перетаскивания ноды
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
            // Если курсор ушел за пределы окна, прекращаем изменение размеров
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
                        // Новая ширина панели вычисляется как расстояние от правой границы окна до мыши
                        window.panelWidth = window.position.width - e.mousePosition.x;
                        // Ограничиваем panelWidth от minPanelWidth до 2/3 ширины окна
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