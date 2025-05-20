using UnityEngine;
using UnityEngine.UI;

namespace SpaceN.Scripts
{
    public class InputHandler : MonoBehaviour
    {
        // Ссылка на UIManager для доступа к кнопкам
        private UIManager _uiManager;

        private void Start()
        {
            // Получаем ссылку на UIManager
            _uiManager = FindObjectOfType<UIManager>();
            if (_uiManager == null)
            {
                Debug.LogError("UIManager не найден!");
            }
        }

        void Update()
        {
            // Отслеживаем нажатия клавиш 1-6 для эмуляции нажатия на кнопки
            for (int i = 0; i < 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    Debug.Log($"Нажата клавиша {i + 1}");
                    SimulateButtonClick(i);
                }
            }

            // Нажатие ESC для выхода из игры
            if (Input.GetKeyDown(KeyCode.Escape))
                QuitGame();
        }

        private void SimulateButtonClick(int index)
        {
            if (_uiManager == null || _uiManager.optionButtons == null || _uiManager.optionButtons.Length <= index)
            {
                Debug.LogWarning("Кнопки не инициализированы или индекс выходит за пределы массива.");
                return;
            }

            Button button = _uiManager.optionButtons[index];
            if (button != null && button.interactable)
            {
                button.onClick.Invoke(); // Эмулируем нажатие на кнопку
            }
            else
            {
                Debug.LogWarning($"Кнопка {index + 1} неактивна или отсутствует.");
            }
        }

        private void QuitGame()
        {
            Debug.Log("Выход из игры...");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}