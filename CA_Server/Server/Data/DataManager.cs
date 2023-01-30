using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Data
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        public static Dictionary<int, Data.Character> CharacterDict { get; private set; } = new Dictionary<int, Data.Character>();

        public static void LoadData()
        {
            CharacterDict = LoadJson<Data.CharacterData, int, Data.Character>("CharacterData").MakeDict();
        }

        private static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }
}