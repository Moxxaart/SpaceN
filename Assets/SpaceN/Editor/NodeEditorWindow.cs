using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SpaceN.Scripts;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace SpaceN.Editor
{
    public class NodeEditorWindow : EditorWindow
    {
        public List<Node> nodes { get; set; } = new List<Node>();
        private Vector2 offset;
        private Vector2 drag;
        private bool stylesInitialized = false;
        public Node activeNode { get; set; }

        // Кэшированные GUI стили
        private GUIStyle customToolbarButton;
        private GUIStyle customToolbarStyle;
    
        public bool isDragging { get; set; } = false; // Флаг для отслеживания перетаскивания
        public Vector2 mouseDownPosition { get; set; }
        public float clickThreshold { get; set; } = 5f; // Порог в пикселях для различия клика и drag
        public bool isDraggingNode { get; set; } = false; // Флаг для отслеживания перетаскивания ноды
    
        // Параметры правой панели
        public bool isResizingPanel { get; set; } = false;
        // Дополнительные поля для правой панели:
        public float panelWidth { get; set; } = 0; // если 0, будет инициализировано как minPanelWidth (свернутая)
        public float minPanelWidth { get; set; } = UILayout.RightPanelMinWidth;
        public float maxPanelWidthFraction { get; set; } = UILayout.RightPanelFraction; // максимум = 2/3 ширины окна
        private float defaultPanelWidth = UILayout.RightPanelWidth; // значение, которое будет восстановлено при разворачивании
        private readonly float panelResizeAreaWidth = UILayout.RightPanelResizeAreaWidth;
        private Vector2 scrollPosition = Vector2.zero;
        
        private const string LocalizationFileName = "Event_Localization";
        private const string ResourcesRoot = "Assets/SpaceN/Resources/";
        public static List<string> Languages = new List<string>(LocalizationLanguages.GetLanguageNames());
        public static string CurrentLanguage = LocalizationLanguages.CurrentLanguageName;

        private const string ImagesRootPath = "Assets/SpaceN/Resources/Images/Locations/";
        private const string DefaultImagePath = "Assets/SpaceN/Resources/Images/Locations/default.png";

        public string global_location;
        public string location;
        public string sublocation;

        private bool hasTextFieldFocus = false;

        private EventProcessor eventProcessor;
        private void OnEnable()
        {
            eventProcessor = new EventProcessor(this);
            LocationManager.LoadLocationData();
        }

        [MenuItem("Window/Node Editor")]
        public static void OpenWindow()
        {
            NodeEditorWindow window = GetWindow<NodeEditorWindow>();
            window.titleContent = new GUIContent("Node Editor");
        }

        private void OnGUI()
        {
            // Сбрасываем флаг фокуса в начале кадра
            hasTextFieldFocus = false;

            // Инициализация panelWidth, если не задано
            if (panelWidth <= 0)
                panelWidth = defaultPanelWidth;
        
            if (!stylesInitialized)
            {
                InitializeStyles();
                stylesInitialized = true;
            } 
        
            EditorGUIUtils.DrawGrid(20, 0.2f, Color.gray, offset, drag, position);
            EditorGUIUtils.DrawGrid(100, 0.4f, Color.gray, offset, drag, position);

            // Отрисовка верхней панели меню
            DrawToolbar();
        
            // Основная область редактора (без правой панели)
            Rect mainArea = new Rect(0, 0, position.width - panelWidth, position.height);
            GUILayout.BeginArea(mainArea);
            EditorGUIUtils.DrawConnections(nodes);
            DrawNodes();

            // Обработка событий через EventProcessor
            Event e = Event.current;
            eventProcessor.ProcessEvents(Event.current);
            eventProcessor.ProcessNodeEvents(Event.current);

            // Удаление нод, помеченных на удаление
            nodes.RemoveAll(n => n.toBeDeleted);
            GUILayout.EndArea();
        
            // Отрисовка правой панели
            DrawRightPanel();

            // Если в этом кадре не было фокуса на текстовом поле, 
            // но он был в предыдущем - сбрасываем фокус
            //if (!hasTextFieldFocus && GUIUtility.keyboardControl != 0)
            //{
            //    ClearFocus();
            //}

            if (GUI.changed)
                Repaint();
        }

        private void InitializeStyles()
        {
            try
            {
                // Если стили еще не инициализированы, ждем следующего вызова OnGUI
                if (EditorStyles.label == null) 
                {
                    Debug.LogWarning("EditorStyles еще не инициализированы. Повторная попытка в OnGUI.");
                    return;
                }

                // Теперь можно безопасно получить стили
                var toolbarButtonStyle = EditorStyles.toolbarButton ?? new GUIStyle();
                if (toolbarButtonStyle == null)
                {
                    Debug.LogWarning("EditorStyles.toolbarButton вернул null, используется fallback стиль.");
                }
            
                customToolbarButton = UILayout.ToolbarButtonStyle;
                customToolbarStyle = UILayout.ToolbarStyle;
            
                // Аналогично для toolbar
                var toolbarStyle = EditorStyles.toolbar ?? new GUIStyle();
                if (toolbarStyle == null)
                {
                    Debug.LogWarning("EditorStyles.toolbar вернул null, используется fallback стиль.");
                }
            
                customToolbarStyle.normal.background = EditorGUIUtils.CreateColorTexture(Color.black);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка при инициализации стилей: {ex.Message}");
            }
        }


    
        // Добавляем в NodeEditorWindow
        private void LoadAllNodes()
        {
            nodes.Clear();
            var idSet = new HashSet<string>();

            // Загружаем локализацию для текущего языка
            EditorLocalizationManager.Instance.LoadLocalizationForLanguage(EditorLocalizationManager.CurrentLanguage);
        
            string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/SpaceN/Resources/Locations" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetExtension(path) != ".json") continue;

                TextAsset jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                GameData data = JsonUtility.FromJson<GameData>(jsonFile.text);
        
                foreach (EventData eventData in data.events)
                {
                    // Skip duplicates
                    if (idSet.Contains(eventData.id))
                    {
                        Debug.LogWarning($"Duplicate node ID found: {eventData.id}");
                        continue;
                    }

                    idSet.Add(eventData.id);

                    // Убедимся, что location инициализирована
                    eventData.location ??= new LocationData();

                    // Создаем ноду для каждого события
                    Node node = new Node(
                        EditorGUIUtils.CalculateNodePosition(nodes.Count),
                        200,
                        100,
                        eventData,
                        data
                    );
                    nodes.Add(node);
                }
            }

            RebuildConnections();
            GUI.changed = true;
        }

        private void RebuildConnections()
        {
            try
            {
                var nodeMap = new Dictionary<string, Node>();

                // First pass - build dictionary with duplicate checking
                foreach (Node node in nodes)
                {
                    if (nodeMap.ContainsKey(node.id))
                    {
                        Debug.LogError($"Duplicate node ID detected: {node.id}");
                        continue;
                    }
                    nodeMap[node.id] = node;
                }

                // Second pass - build connections
                foreach (Node node in nodes)
                {
                    node.connectedNodeIds.Clear();
                    if (node.eventData?.options == null)
                        continue;

                    foreach (OptionData option in node.eventData.options)
                    {
                        if (!string.IsNullOrEmpty(option.target))
                        {
                            if (nodeMap.TryGetValue(option.target, out Node targetNode))
                            {
                                node.connectedNodeIds.Add(option.target);
                            }
                            else
                            {
                                Debug.LogWarning($"Invalid target node ID: {option.target}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error rebuilding connections: {ex.Message}");
            }
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(customToolbarStyle, GUILayout.Height(UILayout.ToolbarHeight));

            if (GUILayout.Button("Load Nodes", customToolbarButton, GUILayout.ExpandWidth(false)))
            {
                OnLoadNodes();
            }
            if (GUILayout.Button("Save Nodes", customToolbarButton, GUILayout.ExpandWidth(false)))
            {
                OnSaveNodes();
            }
            if (GUILayout.Button("Clear All", customToolbarButton, GUILayout.ExpandWidth(false)))
            {
                OnClearNodes();
            }

            // Выбор языка
            DrawLanguageSelection();
            GUILayout.EndHorizontal();
        }
    
        void OnLoadNodes()
        {
            LoadAllNodes();
        }
    
        void OnSaveNodes()
        {
            if (EditorUtility.DisplayDialog("Save Confirmation",
            "Save all changes to files?\nThis will overwrite existing data.",
            "Save", "Cancel"))
            {
                SaveNodesToFile(EditorLocalizationManager.CurrentLanguage);
            }
            //SaveNodesToFile(CurrentLanguage);
        }

        void OnClearNodes()
        {
            nodes.Clear();
            activeNode = null;
            Repaint();
        }

        void DrawLanguageSelection()
        {
            int currentLangIndex = System.Array.IndexOf(LocalizationLanguages.GetLanguageNames(), EditorLocalizationManager.CurrentLanguage);
            if (currentLangIndex < 0) currentLangIndex = 0;

            int selectedIndex = EditorGUILayout.Popup(currentLangIndex, LocalizationLanguages.GetLanguageNames(), GUILayout.Width(100));
            if (selectedIndex != currentLangIndex)
            {
                EditorLocalizationManager.CurrentLanguage = LocalizationLanguages.GetLanguageNames()[selectedIndex];
                LocationManager.SetCurrentLanguage(EditorLocalizationManager.CurrentLanguage); // Обновляем язык в LocationManager
                Debug.Log("Выбран язык: " + EditorLocalizationManager.CurrentLanguage);
                EditorLocalizationManager.Instance.LoadLocalizationForLanguage(EditorLocalizationManager.CurrentLanguage);
            }
        }

        public void ClearFocus()
        {
            if (GUIUtility.hotControl == 0)
            {
                GUI.FocusControl(null);
                EditorGUIUtility.editingTextField = false;
                Repaint();
            }
        }

        void DrawNodes()
        {
            if (nodes != null)
            {
                foreach (Node node in nodes)
                {
                    NodeRenderer renderer = new NodeRenderer(node);
                    renderer.Draw();

                    if (node == activeNode)
                    {
                        DrawNodeOutline(node);
                    }
                }
            }
        }

        void DrawNodeOutline(Node node)
        {
            Rect r = node.rect;
            Vector3 topLeft = new Vector3(r.x, r.y);
            Vector3 topRight = new Vector3(r.x + r.width, r.y);
            Vector3 bottomRight = new Vector3(r.x + r.width, r.y + r.height);
            Vector3 bottomLeft = new Vector3(r.x, r.y + r.height);
    
            Handles.BeginGUI();
            Handles.color = UILayout.NodeOutlineColor;
            Handles.DrawAAPolyLine(1, new Vector3[] { topLeft, topRight, bottomRight, bottomLeft, topLeft });
            Handles.EndGUI();
        }
    
        // Метод для поиска ноды по точке (пример, если его ещё нет)
        public Node GetNodeAtPoint(Vector2 point)
        {
            foreach (var node in nodes)
            {
                if (node.rect.Contains(point))
                    return node;
            }
            return null;
        }

        public void OnDrag(Vector2 delta)
        {
            // Если идет изменение размера панели, не перетаскиваем рабочую область
            if (isResizingPanel)
                return;
        
            // Если курсор находится в области правой панели, не обрабатываем перетаскивание рабочих нод.
            if (Event.current.mousePosition.x > position.width - panelWidth)
                return;

            drag = delta;
            if (nodes != null)
            {
                foreach (Node node in nodes)
                {
                    node.Drag(delta);
                }
            }
            GUI.changed = true;
        }

        // Метод для показа контекстного меню
        public void ShowContextMenu(Vector2 mousePosition)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Node"), false, () => AddNode(mousePosition));
            menu.ShowAsContext();
        }

        public void AddNode(Vector2 position)
        {
            if (nodes == null)
                nodes = new List<Node>();

            // Генерируем уникальный ID
            string newNodeID;
            do
            {
                newNodeID = $"evt_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            } while (nodes.Any(n => n.id == newNodeID));

            string descKey = $"{newNodeID}_desc";

            // Создаем полностью инициализированную EventData
            EventData newEvent = new EventData
            {
                id = newNodeID,
                description = descKey,
                image = "default",
                location = new LocationData
                {
                    global = LocationManager.GetGlobalLocations().FirstOrDefault() ?? "",
                    local = LocationManager.GetLocations().FirstOrDefault() ?? "",
                    sub = LocationManager.GetSublocations().FirstOrDefault() ?? ""
                },
                effects = new List<EventEffect>(),
                options = new List<OptionData>()
            };

            // Создаем ноду
            Node newNode = new Node(
                position,
                UILayout.NodeWidth,
                UILayout.NodeHeight,
                newEvent,
                null
            )
            {
                title = newNodeID, // Не используется, оставлено для совместимости
                description = descKey
            };

            nodes.Add(newNode);
            activeNode = newNode;

            // Добавляем локализацию
            EditorLocalizationManager.Instance.UpdateLocalization(newNodeID, "New Node");
            EditorLocalizationManager.Instance.UpdateLocalization(descKey, "New Node Description");

            GUI.changed = true;
        }

        string GetNextNodeID()
        {
            HashSet<int> usedIDs = new HashSet<int>();
            foreach (var node in nodes)
            {
                if (node.id.StartsWith("node_"))
                {
                    if (int.TryParse(node.id.Substring(5), out int id))
                    {
                        usedIDs.Add(id);
                    }
                }
            }

            // Ищем первый свободный ID
            int nextID = 0;
            while (usedIDs.Contains(nextID))
            {
                nextID++;
            }

            return "node_" + nextID;
        }

        // ======= Рисование правой панели =======
        void DrawRightPanel()
        {
            if (panelWidth < minPanelWidth + 1f)
            {
                DrawCollapsedPanel();
            }
            else
            {
                DrawExpandedPanel();
            }
        }

        void DrawCollapsedPanel()
        {
            Rect buttonRect = new Rect(position.width - minPanelWidth, 0, minPanelWidth, position.height);
            EditorGUI.DrawRect(buttonRect, UILayout.RightPanelBackgroundColor); // фон кнопки
            if (GUI.Button(buttonRect, "[<]"))
            {
                panelWidth = defaultPanelWidth;
            }
            EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.ResizeHorizontal);
            eventProcessor.ProcessPanelResize(Event.current, buttonRect);
        }

        void DrawExpandedPanel()
        {
            Rect panelRect = new Rect(position.width - panelWidth, 0, panelWidth, position.height);
            EditorGUI.DrawRect(panelRect, UILayout.RightPanelBackgroundColor);

            GUILayout.BeginArea(panelRect);
            DrawPanelHeader();
            if (activeNode != null)
            {
                DrawNodeEditor();
            }
            else
            {
                GUILayout.Label("Нет выбранной ноды");
            }
            GUILayout.EndArea();

            DrawPanelOutline(panelRect);
            DrawResizeHandle(panelRect);
        }

        void DrawPanelHeader()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("[>]"))
            {
                panelWidth = minPanelWidth;
            }
            if (GUILayout.Button("[X]"))
            {
                panelWidth = minPanelWidth;
                activeNode = null;
            }
            GUILayout.EndHorizontal();
        }

        void DrawNodeEditor()
        {
            if (activeNode == null || activeNode.eventData == null)
            {
                GUILayout.Label("No node selected or node data is invalid");
                return;
            }

            EditorGUI.BeginChangeCheck();

            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Active Node: " + activeNode.id, UILayout.BoldLabelStyle);
                GUILayout.EndHorizontal();

                DrawNodePreview();
                DrawNodeFields();
            }
            finally
            {
                if (EditorGUI.EndChangeCheck())
                {
                    GUI.changed = true;
                }
            }
        }

        void DrawNodePreview()
        {
            GUILayout.BeginHorizontal();

            // Создаем область для клика по изображению
            Rect imageRect = new Rect(270, 20, 100, 100);

            // Загружаем превью
            Texture2D preview = null;
            if (!string.IsNullOrEmpty(activeNode.eventData?.image))
            {
                string resourcePath = $"Images/Locations/{activeNode.eventData.image}";
                preview = Resources.Load<Texture2D>(resourcePath);
            }

            // Отрисовываем превью или заглушку
            if (preview != null)
            {
                GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.DrawRect(imageRect, new Color(0.2f, 0.2f, 0.2f));
                GUI.Label(imageRect, "No Image", EditorStyles.centeredGreyMiniLabel);
            }

            // Обрабатываем клик по области изображения
            if (Event.current.type == EventType.MouseDown && imageRect.Contains(Event.current.mousePosition))
            {
                string selectedImagePath = EditorUtility.OpenFilePanel("Select Event Image",
                    ImagesRootPath, "png,jpg,jpeg");

                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    // Преобразуем абсолютный путь в путь относительно Assets
                    if (selectedImagePath.StartsWith(Application.dataPath))
                    {
                        selectedImagePath = "Assets" + selectedImagePath.Substring(Application.dataPath.Length);
                    }

                    // Проверяем, что изображение в правильной папке
                    if (selectedImagePath.StartsWith("Assets/SpaceN/Resources/Images/Locations/"))
                    {
                        // Получаем имя файла без расширения
                        string fileName = Path.GetFileNameWithoutExtension(selectedImagePath);
                        activeNode.eventData.image = fileName;
                        activeNode.imageName = fileName;
                        GUI.changed = true;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Location",
                            "Image must be in Assets/SpaceN/Resources/Images/Locations/ folder", "OK");
                    }
                }

                Event.current.Use();
            }

            GUILayout.BeginVertical();
            DrawDropdowns();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        void DrawDropdowns()
        {
            //----------------GLOBAL------------------
            GUILayout.BeginHorizontal();
            GUILayout.Label("Global:", GUILayout.Width(60));
            string[] globalLocNames = LocationManager.GetGlobalLocations()
                .Select(tag => LocationManager.GetLocalizedGlobalLocation(tag)).ToArray();
            List<string> globalTags = LocationManager.GetGlobalLocations();

            int currentGlobalIndex = globalTags.IndexOf(activeNode.eventData.location.global);
            if (currentGlobalIndex < 0) currentGlobalIndex = 0; // Предотвращаем OutOfRange

            int newGlobalIndex = EditorGUILayout.Popup(currentGlobalIndex, globalLocNames, GUILayout.Width(170));
            if (newGlobalIndex < globalTags.Count) // Предотвращаем OutOfRange
                activeNode.eventData.location.global = globalTags[newGlobalIndex];
            GUILayout.EndHorizontal();
            //----------------LOCAL------------------
            GUILayout.BeginHorizontal();
            GUILayout.Label("Location:", GUILayout.Width(60));
            string[] locNames = LocationManager.GetLocations()
                .Select(tag => LocationManager.GetLocalizedLocation(tag)).ToArray();
            List<string> locTags = LocationManager.GetLocations();

            int currentLocIndex = locTags.IndexOf(activeNode.eventData.location.local);
            if (currentLocIndex < 0) currentLocIndex = 0; // Предотвращаем OutOfRange

            int newLocIndex = EditorGUILayout.Popup(currentLocIndex, locNames, GUILayout.Width(170));
            if (newLocIndex < locTags.Count) // Предотвращаем OutOfRange
                activeNode.eventData.location.local = locTags[newLocIndex];
            GUILayout.EndHorizontal();
            //----------------SUB------------------
            GUILayout.BeginHorizontal();
            GUILayout.Label("Subloc:", GUILayout.Width(60));
            string[] subLocNames = LocationManager.GetSublocations()
                .Select(tag => LocationManager.GetLocalizedLocation(tag)).ToArray();
            List<string> subLocTags = LocationManager.GetSublocations();

            int currentSubLocIndex = subLocTags.IndexOf(activeNode.eventData.location.sub);
            if (currentSubLocIndex < 0) currentSubLocIndex = 0; // Предотвращаем OutOfRange

            int newSubLocIndex = EditorGUILayout.Popup(currentSubLocIndex, subLocNames, GUILayout.Width(170));
            if (newSubLocIndex < subLocTags.Count) // Предотвращаем OutOfRange
                activeNode.eventData.location.sub = subLocTags[newSubLocIndex];
            GUILayout.EndHorizontal();
        }

        void DrawNodeFields()
        {
            if (activeNode == null || activeNode.eventData == null) return;

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(10);

            // ID ноды (редактируемое поле)
            GUILayout.BeginHorizontal();
            GUILayout.Label("ID:", GUILayout.Width(30f));
            string newId = EditorGUILayout.TextField(activeNode.id, GUILayout.Width(200f));

            // Проверяем уникальность ID при изменении
            if (newId != activeNode.id)
            {
                if (string.IsNullOrEmpty(newId))
                {
                    EditorGUILayout.HelpBox("ID cannot be empty", MessageType.Error);
                }
                else if (nodes.Any(n => n != activeNode && n.id == newId))
                {
                    EditorGUILayout.HelpBox("ID must be unique", MessageType.Error);
                }
                else
                {
                    // Обновляем ID во всех связанных опциях
                    UpdateOptionIds(activeNode.id, newId);

                    // Обновляем ключ описания
                    string oldDescKey = $"{activeNode.id}_desc";
                    string newDescKey = $"{newId}_desc";
                    EditorLocalizationManager.Instance.RenameLocalizationKey(oldDescKey, newDescKey,
                        EditorLocalizationManager.Instance.GetLocalizedText(oldDescKey));

                    activeNode.id = newId;
                    activeNode.eventData.id = newId;
                    activeNode.description = newDescKey;
                    activeNode.eventData.description = newDescKey;

                    GUI.changed = true;
                }
            }
            GUILayout.EndHorizontal();

            // Заголовок ноды (локализованный текст)
            GUILayout.BeginHorizontal();
            GUILayout.Label("Title:", GUILayout.Width(30f));
            string currentTitle = EditorLocalizationManager.Instance.GetLocalizedText(activeNode.id);
            string newTitle = EditorGUILayout.TextField(currentTitle, GUILayout.Width(200f));
            GUILayout.EndHorizontal();

            // Описание ноды (локализованный текст)
            string currentDesc = EditorLocalizationManager.Instance.GetLocalizedText($"{activeNode.id}_desc");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
            string newDesc = EditorGUILayout.TextArea(currentDesc, EditorStyles.textArea, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            // Применяем изменения
            if (EditorGUI.EndChangeCheck())
            {
                // Обновляем заголовок
                EditorLocalizationManager.Instance.UpdateLocalization(activeNode.id, newTitle);
                //EditorLocalizationManager.Instance.UpdateLocalization(activeNode.eventData.id, newTitle);

                // Обновляем описание
                EditorLocalizationManager.Instance.UpdateLocalization($"{activeNode.id}_desc", newDesc);

                GUI.changed = true;
            }

            // Отрисовка опций
            DrawOptionsSection();

            if (GUILayout.Button("Save Changes"))
            {
                SaveCurrentChanges();
                ClearFocus();
            }
        }

        private void UpdateOptionIds(string oldNodeId, string newNodeId)
        {
            // Обновляем target в опциях других нод
            foreach (var node in nodes)
            {
                if (node.eventData?.options != null)
                {
                    foreach (var option in node.eventData.options)
                    {
                        if (option.target == oldNodeId)
                        {
                            option.target = newNodeId;
                        }
                    }
                }
            }

            // Обновляем ID в опциях текущей ноды
            if (activeNode.eventData?.options != null)
            {
                foreach (var option in activeNode.eventData.options)
                {
                    if (option.id.StartsWith(oldNodeId + "_opt"))
                    {
                        string oldKey = option.id;
                        string newKey = option.id.Replace(oldNodeId + "_opt", newNodeId + "_opt");

                        EditorLocalizationManager.Instance.RenameLocalizationKey(
                            oldKey,
                            newKey,
                            EditorLocalizationManager.Instance.GetLocalizedText(oldKey)
                        );

                        option.id = newKey;
                    }
                }
            }
        }

        void DrawOptionsSection()
        {
            GUILayout.Space(10);
            GUILayout.Label("OPTIONS:", EditorStyles.boldLabel);

            if (activeNode.eventData?.options == null)
                activeNode.eventData.options = new List<OptionData>();

            // Заголовок таблицы
            GUILayout.BeginHorizontal();
            GUILayout.Label("DISPLAY TEXT", GUILayout.Width(200));
            GUILayout.Label("TARGET NODE", GUILayout.Width(150));
            GUILayout.Label("ACTIONS", GUILayout.Width(80));
            GUILayout.EndHorizontal();

            // Отрисовка опций
            for (int i = 0; i < activeNode.eventData.options.Count; i++)
            {
                var option = activeNode.eventData.options[i];
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginHorizontal();

                // Автоматически генерируем ID опции, если его нет
                if (string.IsNullOrEmpty(option.id))
                {
                    option.id = $"{activeNode.id}_opt{i + 1}";
                }

                // Поле для локализованного текста
                string currentText = EditorLocalizationManager.Instance.GetLocalizedText(option.id);
                string newText = EditorGUILayout.TextField(currentText, GUILayout.Width(200));

                // Поле для целевой ноды
                string newTarget = EditorGUILayout.TextField(option.target ?? "", GUILayout.Width(150));

                // Кнопка редактирования условий
                if (GUILayout.Button("Cond", GUILayout.Width(50)))
                {
                    OpenConditionEditorWindow(option);
                }

                // Кнопка удаления
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Option",
                        "Delete this option?", "Delete", "Cancel"))
                    {
                        EditorLocalizationManager.Instance.RemoveLocalizationKey(option.id);
                        activeNode.eventData.options.RemoveAt(i);
                        GUI.changed = true;
                    }
                }

                GUILayout.EndHorizontal();

                // Применяем изменения
                if (EditorGUI.EndChangeCheck())
                {
                    EditorLocalizationManager.Instance.UpdateLocalization(option.id, newText);
                    option.target = newTarget;
                    GUI.changed = true;
                }
            }

            // Кнопка добавления новой опции
            if (activeNode.eventData.options.Count < 6)
            {
                if (GUILayout.Button("+ Add Option", GUILayout.Width(120)))
                {
                    string newKey = $"{activeNode.id}_opt{activeNode.eventData.options.Count + 1}";
                    activeNode.eventData.options.Add(new OptionData
                    {
                        id = newKey,
                        target = ""
                    });

                    EditorLocalizationManager.Instance.UpdateLocalization(newKey, "New Option");
                    GUI.changed = true;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Maximum 6 options allowed", MessageType.Info);
            }
        }

        // Окно редактирования условий
        private void OpenConditionEditorWindow(OptionData option)
        {
            OptionConditionEditorWindow.Open(option, (updatedOption) =>
            {
                // Эта лямбда вызывается при сохранении в редакторе условий
                int index = activeNode.eventData.options.FindIndex(o => o == option);
                if (index >= 0)
                {
                    activeNode.eventData.options[index] = updatedOption;
                    GUI.changed = true;
                }
            });
        }

        void DrawPanelOutline(Rect panelRect)
        {
            Handles.BeginGUI();
            Handles.color = Color.gray;
            Handles.DrawSolidRectangleWithOutline(panelRect, Color.clear, Color.white);
            Handles.EndGUI();
        }

        void DrawResizeHandle(Rect panelRect)
        {
            Rect resizeRect = new Rect(position.width - panelWidth - panelResizeAreaWidth, 0, panelResizeAreaWidth, position.height);
            EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);
            eventProcessor.ProcessPanelResize(Event.current, resizeRect);
            //ProcessPanelResize(Event.current, resizeRect);
        }

        private void SaveNodesToFile(string lang)
        {
            if (nodes == null || nodes.Count == 0)
            {
                Debug.LogError("No nodes to save!");
                return;
            }

            RebuildConnections();

            // Используем AssetDatabase пути вместо Application.dataPath
            string folderPath = "Assets/SpaceN/Resources/Locations";
            string localizationsPath = "Assets/SpaceN/Resources/Localizations";

            // Используем исходное имя файла
            string jsonFilePath = Path.Combine(folderPath, "abadoned_station.json");
            string localeFilePath = Path.Combine(localizationsPath, $"Event_Localization_{lang}.json");

            try
            {
                // 1. Загружаем существующий JSON чтобы сохранить структуру
                TextAsset existingJson = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonFilePath);
                GameData gameData = existingJson != null ?
                    JsonUtility.FromJson<GameData>(existingJson.text) :
                    new GameData();

                // 2. Собираем все ключи локализации
                var localizationData = new Dictionary<string, string>();

                // Загружаем существующую локализацию если есть
                TextAsset existingLocale = AssetDatabase.LoadAssetAtPath<TextAsset>(localeFilePath);
                if (existingLocale != null)
                {
                    localizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(existingLocale.text);
                }

                // 3. Обновляем данные
                gameData.events.Clear(); // Очищаем старые события

                foreach (Node node in nodes)
                {
                    if (node.eventData == null)
                    {
                        node.eventData = new EventData
                        {
                            id = node.id,
                            description = node.description,
                            image = node.imageName,
                            location = new LocationData(),
                            effects = new List<EventEffect>(),
                            options = new List<OptionData>()
                        };
                    }

                    gameData.events.Add(node.eventData);

                    // Обновляем локализацию
                    /*if (!string.IsNullOrEmpty(node.title))
                    {
                        localizationData[node.title] = EditorLocalizationManager.Instance.GetLocalizedText(node.title);
                    }

                    if (!string.IsNullOrEmpty(node.description))
                    {
                        localizationData[node.description] = EditorLocalizationManager.Instance.GetLocalizedText(node.description);
                    }*/

                    // add node title text to localization file
                    if (!string.IsNullOrEmpty(node.eventData.id))
                    {
                        localizationData[node.eventData.id] =
                            EditorLocalizationManager.Instance.GetLocalizedText(node.eventData.id);
                    }

                    // add node desc text to localization file
                    if (!string.IsNullOrEmpty(node.eventData.description))
                    {
                        localizationData[node.eventData.description] =
                            EditorLocalizationManager.Instance.GetLocalizedText(node.eventData.description);
                    }

                    // Локализация опций
                    if (node.eventData.options != null)
                    {
                        foreach (var option in node.eventData.options)
                        {
                            if (!string.IsNullOrEmpty(option.id))
                            {
                                localizationData[option.id] =
                                    EditorLocalizationManager.Instance.GetLocalizedText(option.id);
                            }
                        }
                    }
                }

                // 4. Сериализуем с правильными настройками
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                };

                // 5. Сохраняем файлы
                File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(gameData, settings));
                File.WriteAllText(localeFilePath, JsonConvert.SerializeObject(localizationData, settings));

                AssetDatabase.Refresh();
                Debug.Log($"Data successfully saved to:\n{jsonFilePath}\n{localeFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Save failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SaveCurrentChanges()
        {
            if (activeNode == null) return;

            RebuildConnections();

            // Просто обновляем данные в памяти
            GUI.changed = true;
            Debug.Log("Changes saved to memory (not to disk)");
        }
    }
}
