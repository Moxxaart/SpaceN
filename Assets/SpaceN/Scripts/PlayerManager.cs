using UnityEngine;

namespace SpaceN.Scripts
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Основные параметры игрока")]
        [SerializeField] private int health = 100;
        [SerializeField] private int money = 500;
    
        [Header("Характеристики игрока")]
        [SerializeField] private int strength = 1;
        [SerializeField] private int agility = 1;
        [SerializeField] private int constitution = 1;
        [SerializeField] private int intellect = 1;
        [SerializeField] private int charisma = 1;
    
        [Header("Репутация игрока")]
        [SerializeField] private int reputation = 0;

        public int Health => health;
        private const int maxHealth = 100;
        private const int minHealth = 0;
        public int Money => money;
        private const int maxMoney = 10000000;
        private const int minMoney = 0;
        public int Strength => strength;
        public int Agility => agility;
        public int Constitution => constitution;
        public int Intellect => intellect;
        public int Charisma => charisma;
        public int Reputation => reputation;
        private const int maxParameter = 25;
        private const int minParameter = 1;

    
        // Универсальные геттер и сеттер для параметров
        /*public int GetParameter(string parameter)
    {
        switch (parameter)
        {
            case "Money": return money;
            case "Health": return health;
            // Добавьте другие параметры при необходимости
            default:
                Debug.LogWarning("Неизвестный параметр: " + parameter);
                return 0;
        }
    }*/
    
        public void IncreaseParameter(string parameter, int value)
        {
            switch (parameter)
            {
                case "Money":
                    money = Mathf.Clamp(money + value, minMoney, maxMoney); break;
                case "Health":
                    health = Mathf.Clamp(health + value, minHealth, maxHealth); break;
                case "Strength":
                    strength = Mathf.Clamp(strength + value, minParameter, maxParameter); break;
                case "Agility":
                    agility = Mathf.Clamp(agility + value, minParameter, maxParameter); break;
                case "Constitution":
                    constitution = Mathf.Clamp(constitution + value, minParameter, maxParameter); break;
                case "Intellect":
                    intellect = Mathf.Clamp(intellect + value, minParameter, maxParameter); break;
                case "Charisma":
                    charisma = Mathf.Clamp(charisma + value, minParameter, maxParameter); break;
                // Добавьте другие параметры по мере необходимости
                default:
                    Debug.LogWarning($"Неизвестный параметр: {parameter} | Значение: {value.ToString()}");
                    break;
            }
        }
    
        public void DecreaseParameter(string parameter, int value)
        {
            switch (parameter)
            {
                case "Money":
                    money = Mathf.Clamp(money - value, minMoney, maxMoney); break;
                case "Health":
                    health = Mathf.Clamp(health - value, minHealth, maxHealth); break;
                case "Strength":
                    strength = Mathf.Clamp(strength - value, minParameter, maxParameter); break;
                case "Agility":
                    agility = Mathf.Clamp(agility - value, minParameter, maxParameter); break;
                case "Constitution":
                    constitution = Mathf.Clamp(constitution - value, minParameter, maxParameter); break;
                case "Intellect":
                    intellect = Mathf.Clamp(intellect - value, minParameter, maxParameter); break;
                case "Charisma":
                    charisma = Mathf.Clamp(charisma - value, minParameter, maxParameter); break;
                // Добавьте другие параметры по мере необходимости
                default:
                    Debug.LogWarning($"Неизвестный параметр: {parameter} | Значение: {value.ToString()}");
                    break;
            }
        }
    
        public void SetParameter(string parameter, int value)
        {
            switch (parameter)
            {
                case "Money":
                    money = Mathf.Clamp(value, minMoney, maxMoney); break;
                case "Health":
                    health = Mathf.Clamp(value, minHealth, maxHealth); break;
                case "Strength":
                    strength = Mathf.Clamp(value, minParameter, maxParameter); break;
                case "Agility":
                    agility = Mathf.Clamp(value, minParameter, maxParameter); break;
                case "Constitution":
                    constitution = Mathf.Clamp(value, minParameter, maxParameter); break;
                case "Intellect":
                    intellect = Mathf.Clamp(value, minParameter, maxParameter); break;
                case "Charisma":
                    charisma = Mathf.Clamp(value, minParameter, maxParameter); break;
                // Добавьте другие параметры по мере необходимости
                default:
                    Debug.LogWarning($"Неизвестный параметр: {parameter} | Значение: {value.ToString()}");
                    break;
            }
        }
    }
}
