using UnityEngine;

namespace SpaceN.Scripts
{
    public class DataLoader : MonoBehaviour
    {
        public static GameData LoadGame()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Locations/abadoned_station"); // Without ".json"!

            if (jsonFile != null)
            {
                GameData data = JsonUtility.FromJson<GameData>(jsonFile.text);
                Debug.Log(jsonFile.text);
                Debug.Log("Данные загружены из Resources!");
                return data;
            }
            else
            {
                Debug.LogWarning($"Файл не найден в Resources! {jsonFile}");
                return null;
            }
        }
    }
}
