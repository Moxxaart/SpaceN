using System.Collections.Generic;

namespace SpaceN.Scripts
{
    public class GameModel
    {
        //public string CurrentGlobalLocation { get; set; }
        //public string CurrentLocation { get; set; }
        //public string CurrentSubLocation { get; set; }
        public Dictionary<string, EventData> EventDictionary { get; } = new Dictionary<string, EventData>();
        public EventData CurrentEvent { get; set; }
        public PlayerManager PlayerStats { get; set; }
    }
}