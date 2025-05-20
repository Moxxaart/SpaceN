using System;

namespace SpaceN.Scripts
{
    public static class EventBus
    {
        public static event Action<EventData> OnEventTriggered; // Action to display the event
        public static event Action<string> OnOptionSelected;    // Action for option selection

        public static void PublishEvent(EventData eventData) => OnEventTriggered?.Invoke(eventData);
        public static void SelectOption(string optionId) => OnOptionSelected?.Invoke(optionId);
    }
}