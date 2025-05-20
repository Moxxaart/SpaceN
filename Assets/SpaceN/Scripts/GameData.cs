using System;
using System.Collections.Generic;

namespace SpaceN.Scripts
{
    [Serializable]
    public class GameData
    {
        public List<EventData> events;
    }

    [Serializable]
    public class LocationData
    {
        public string global;
        public string local;
        public string sub;
    }

    [Serializable]
    public class EventData
    {
        public string id;
        public string description;
        public string image;
        public LocationData location;
        // Эффекты, которые срабатывают при входе в эвент (необязательно)
        public List<EventEffect> effects;
        public List<OptionData> options;
    }

    [System.Serializable]
    public class DescriptionData {
        public string text; // текст-шаблон с маркерами, напр. "Вы нашли $Money кредитов, теперь у вас $PMoney кредитов."
        public ParameterData parameters; // объект параметров, который может задавать диапазоны и фиксированные значения
    }

    [System.Serializable]
    public class ParameterData {
        // Здесь можно использовать словарь, но словари в Unity не сериализуются напрямую,
        // поэтому можно сделать список пар, или создать отдельные поля для каждого параметра.
        // Если параметров мало, можно сделать так:
        public RangeData Money;
        // Можно добавить и другие параметры, если нужно
    }

    [System.Serializable]
    public class RangeData {
        public int min;
        public int max;
        // Если требуется фиксированное значение, можно добавить:
        public int fixedValue;
    }

    [System.Serializable]
    public class OptionData {
        public string id;
        public string target;
        public List<ConditionData> conditions; // список условий, может быть пустым
        public List<EventEffect> effects;      // список эффектов, может быть пустым
    }

    [System.Serializable]
    public class EventEffect {
        public string type;      // "IncreaseParameter" или "DecreaseParameter"
        public string parameter; // Например, "Money"
        public int value;        // Значение для изменения параметра (например, 100)
    }

    [System.Serializable]
    public class ConditionData {
        public string type;      // например, "HasAtLeast" (проверка наличия определенного значения)
        public string parameter; // например, "Money", "Health", "Reputation" и т.д.
        public int value;        // требуемое значение
    }
}