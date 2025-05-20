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
                Debug.LogError("UIManager �� ������!");
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

                // ��������� ��������� �������
                _controller.LoadInitialEvent(_initialEventId);
                Debug.Log($"���� ���������. ��������� �������: {_initialEventId}");
            }
            else
            {
                Debug.LogError("�� ������� ��������� ������ ����!");
            }
        }

        private void BuildEventDictionary(GameData data)
        {
            if (data?.events == null)
            {
                Debug.LogError("��� ������ ������� ��� ��������!");
                return;
            }

            _model.EventDictionary.Clear();

            foreach (var evt in data.events)
            {
                if (evt == null || string.IsNullOrEmpty(evt.id))
                {
                    Debug.LogWarning("���������� ������� ��� ID, ����������");
                    continue;
                }

                if (_model.EventDictionary.ContainsKey(evt.id))
                {
                    Debug.LogWarning($"������������ ������� � id {evt.id}!");
                    continue;
                }

                // �������������� location, ���� ��� null
                evt.location ??= new LocationData();

                _model.EventDictionary.Add(evt.id, evt);
            }

            Debug.Log($"��������� {_model.EventDictionary.Count} �������");
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
                    Debug.LogWarning($"������������ ������ � id {evt.id}!");
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