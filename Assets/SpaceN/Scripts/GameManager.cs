using UnityEngine;

namespace SpaceN.Scripts
{
    public class GameManager : MonoBehaviour
    {
        private GameModel _model;
        private GameController _controller;
        private UIManager _uiManager;

        [SerializeField] private string _initialEventId = "evt_abadoned_station";
        [SerializeField] private string _defaultLanguage = "ru";

        void Awake()
        {
            // Initialize the model and controller before the UIManager starts working
            _model = new GameModel
            {
                PlayerStats = FindObjectOfType<PlayerManager>()
            };

            _uiManager = FindObjectOfType<UIManager>();
            if (_uiManager == null)
            {
                Debug.LogError("UIManager не найден!");
                return;
            }

            _controller = new GameController(_model, _uiManager);
        }

        void Start()
        {
            GameData loadedData = DataLoader.LoadGame();
            if (loadedData != null)
            {
                LocalizationManager.Instance.LoadLocalizationForLanguage(_defaultLanguage);
                BuildEventDictionary(loadedData);

                // Загружаем начальное событие
                _controller.LoadInitialEvent(_initialEventId);
                Debug.Log($"Игра загружена. Начальное событие: {_initialEventId}");
            }
            else
            {
                Debug.LogError("Не удалось загрузить данные игры!");
            }
        }

        private void BuildEventDictionary(GameData data)
        {
            if (data?.events == null)
            {
                Debug.LogError("Нет данных событий для загрузки!");
                return;
            }

            _model.EventDictionary.Clear();

            foreach (var evt in data.events)
            {
                if (evt == null || string.IsNullOrEmpty(evt.id))
                {
                    Debug.LogWarning("Обнаружено событие без ID, пропускаем");
                    continue;
                }

                if (_model.EventDictionary.ContainsKey(evt.id))
                {
                    Debug.LogWarning($"Дублирование события с id {evt.id}!");
                    continue;
                }

                // Инициализируем location, если она null
                evt.location ??= new LocationData();

                _model.EventDictionary.Add(evt.id, evt);
            }

            Debug.Log($"Загружено {_model.EventDictionary.Count} событий");
        }

        public GameController GetController() => _controller;
        public GameModel GetModel() => _model;

        void OnDestroy()
        {
            _controller?.Dispose();
        }

        /*private void BuildEventDictionary(GameData data)
        {
            foreach (var evt in data.events)
            {
                if (!_model.EventDictionary.ContainsKey(evt.id))
                    _model.EventDictionary.Add(evt.id, evt);
                else
                    Debug.LogWarning($"Дублирование эвента с id {evt.id}!");
            }
        }*/

        /*public GameController GetController()
        {
            return _controller;
        }
    
        public GameModel GetModel()
        {
            return _model;
        }*/
    }
}