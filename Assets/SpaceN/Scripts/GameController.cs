using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace SpaceN.Scripts
{
    public class GameController : IDisposable
    {
        private GameModel _model;
        private UIManager _uiManager;

        public GameController(GameModel model, UIManager uiManager)
        {
            _model = model;
            _uiManager = uiManager;

            // Subscribe to the option selection event
            EventBus.OnOptionSelected += OnOptionSelected;
        }

        public void LoadInitialEvent(string eventId)
        {
            if (_model.EventDictionary.TryGetValue(eventId, out EventData eventData))
            {
                EventBus.PublishEvent(eventData);
                _model.CurrentEvent = eventData;
                _uiManager.DisplayEvent(eventData);
            }
            else
            {
                Debug.LogError($"Событие с id '{eventId}' не найдено!");
            }
        }

        public void LoadNextEvent(string eventId)
        {
            if (_model.EventDictionary.TryGetValue(eventId, out EventData nextEvent))
            {
                // Apply effects, if any
                foreach (var optionData in nextEvent.options)
                {
                    if (optionData.effects != null)
                    {
                        // Check condition if any
                        if (optionData.conditions != null && !AreConditionsMet(optionData.conditions))
                        {
                            Debug.Log("Условия не выполнены. Переход невозможен.");
                            return;
                        }
                    
                        Debug.Log($"EntryEFFECT: {optionData.effects}");
                        ApplyEffects(optionData.effects);
                    }
                }
                EventBus.PublishEvent(nextEvent);
            }
            else
            {
                Debug.LogError($"Событие {eventId} не найдено!");
            }
        
            /*if (nextEvent.options != null)
            {
                _model.CurrentGlobalLocation = changeLocation.global_location;
                _model.CurrentLocation = changeLocation.location;
                _model.CurrentSubLocation = changeLocation.sublocation;
                Debug.Log($"Игрок сменил локацию на: {_model.CurrentGlobalLocation}/{_model.CurrentLocation}/{_model.CurrentSubLocation}");

                //string localizationFolder = "Localizations";
                //LocalizationManager.Instance.LoadLocalizationFromFolder(localizationFolder);
                LocalizationManager.Instance.LoadLocalizationForLanguage("ru"); // Or other language
            }*/
        }
    
        private void OnOptionSelected(string optionId)
        {
            // Search for an option by the passed identifier (option text)
            OptionData option = FindOptionById(optionId);
            if (option != null)
            {
                SelectOption(option);
            }
            else
            {
                Debug.LogWarning($"Опция с id '{optionId}' не найдена.");
            }
        }
    
        public void SelectOption(OptionData option)
        {
            if (!AreConditionsMet(option.conditions))
            {
                Debug.Log("Условия не выполнены для выбранной опции.");
                return;
            }
        
            if (option.effects != null && option.effects.Count > 0)
            {
                Debug.Log($"Применяем {option.effects.Count} эффект(а) опции.");
                ApplyEffects(option.effects);
            }

            // Переходим к следующему событию через result
            if (!string.IsNullOrEmpty(option.target))
            {
                LoadNextEvent(option.target);
            }
            else
            {
                Debug.LogWarning("У опции не указан target для перехода!");
            }
        }

        private OptionData FindOptionById(string optionId)
        {
            // Go through all the options of the current event and look for a match on the text field
            if (_model.CurrentEvent != null && _model.CurrentEvent.options != null)
                return null;

            foreach (OptionData option in _model.CurrentEvent.options)
                {
                    if (option.id == optionId)
                    {
                        return option;
                    }
                }
            return null;
        }

        public void Dispose()
        {
            // Отписываемся от событий
            EventBus.OnOptionSelected -= OnOptionSelected;

            // Можно добавить дополнительную очистку ресурсов при необходимости
            _model = null;
            _uiManager = null;
        }

        public bool AreConditionsMet(List<ConditionData> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            foreach (ConditionData cond in conditions)
            {
                switch (cond.type)
                {
                    case "HasAtLeast":
                        if (cond.parameter == "Money" && _model.PlayerStats.Money < cond.value)
                            return false;
                        break;
                    default:
                        Debug.LogWarning("Неизвестный тип условия: " + cond.type);
                        break;
                }
            }
            return true;
        }

        public void ApplyEffects(List<EventEffect> effects)
        {
            if (effects == null || effects.Count == 0)
                return;

            foreach (EventEffect effect in effects)
            {
                switch (effect.type)
                {
                    case "IncreaseParameter":
                        _model.PlayerStats.IncreaseParameter(effect.parameter, effect.value);
                        break;
                    case "DecreaseParameter":
                        _model.PlayerStats.DecreaseParameter(effect.parameter, effect.value);
                        break;
                    case "SetParameter":
                        _model.PlayerStats.SetParameter(effect.parameter, effect.value);
                        break;
                    default:
                        Debug.LogWarning("Неизвестный тип эффекта: " + effect.type);
                        break;
                }
            }
        }
    }
} 