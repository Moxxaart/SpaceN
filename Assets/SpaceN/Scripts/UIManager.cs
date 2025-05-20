using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceN.Scripts
{
    public class UIManager : MonoBehaviour
    {
        private GameController _gameController;
    
        public Image eventImage;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public Button[] optionButtons;
    
        private void Start()
        {
        
        }

        private void Awake()
        {
            Debug.Log("Awake вызван в UIManager");
            _gameController = FindObjectOfType<GameManager>().GetController();
            if (_gameController == null)
            {
                Debug.LogError("GameController не найден!");
            }
            else
            {
                Debug.Log("GameController успешно инициализирован.");
            }
            EventBus.OnEventTriggered += DisplayEvent;
            EventBus.OnOptionSelected += HandleOptionSelected;
        }

        private void OnDestroy()
        {
            EventBus.OnEventTriggered -= DisplayEvent;
            EventBus.OnOptionSelected -= HandleOptionSelected;
        }

        public void DisplayEvent(EventData evt)
        {
            string localizedTitle = LocalizationManager.Instance.GetLocalizedText(evt.id);
            string localizedDescription = LocalizationManager.Instance.GetLocalizedText(evt.description);
            titleText.text = localizedTitle;
            descriptionText.text = localizedDescription;
            Debug.Log($"Событие: {localizedTitle} - {localizedDescription}");

            if (!string.IsNullOrEmpty(evt.image))
            {
                string imagePath = $"Images/Locations/{evt.image}";
                Sprite loadedSprite = Resources.Load<Sprite>(imagePath);

                if (loadedSprite != null)
                {
                    eventImage.sprite = loadedSprite;
                    eventImage.gameObject.SetActive(true);
                    Debug.Log($"Загружено Ы изображение: {imagePath}");
                }
                else
                {
                    eventImage.gameObject.SetActive(false);
                    Debug.LogWarning($"Изображение {imagePath} не найдено!");
                }
            }
            else
            {
                eventImage.gameObject.SetActive(false);
                Debug.LogWarning("У события нет указанного изображения.");
            }

            UpdateOptions(evt.options);
        }

        private void UpdateOptions(List<OptionData> options)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;

                if (index < options.Count)
                {
                    string localizedOption = LocalizationManager.Instance.GetLocalizedText(options[index].id);
                    optionButtons[index].gameObject.SetActive(true);
                    optionButtons[index].GetComponentInChildren<TextMeshProUGUI>().text = index + 1 + ". " + localizedOption;

                    string nextEventId = options[index].target;
                    //ChangeLocationData locationChange = options[index].changeLocation?.FirstOrDefault();

                    optionButtons[index].onClick.RemoveAllListeners();
                    optionButtons[index].onClick.AddListener(() => EventBus.SelectOption(nextEventId));
                }
                else
                {
                    optionButtons[index].gameObject.SetActive(false);
                }
            }
        }

        private void HandleOptionSelected(string optionId)
        {
            if (_gameController == null)
            {
                Debug.LogError("GameController не инициализирован!");
                return;
            }

            Debug.Log($"Выбрана опция: {optionId}");
            _gameController.LoadNextEvent(optionId); // Загружаем следующее событие
        }
    }
}