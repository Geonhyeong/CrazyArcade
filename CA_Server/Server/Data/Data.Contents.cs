using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    #region Character
    [Serializable]
    public class Character
    {
        public int id;
        public string name;
        public int maxSpeedLvl;
        public int maxPower;
        public int maxBubble;
    }

    [Serializable]
    public class CharacterData : ILoader<int, Character>
    {
        public List<Character> characters = new List<Character>();

        public Dictionary<int, Character> MakeDict()
        {
            Dictionary<int, Character> dict = new Dictionary<int, Character>();
            foreach (Character character in characters)
                dict.Add(character.id, character);
            return dict;
        }
    }
    #endregion
}