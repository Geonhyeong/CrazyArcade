using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Player : GameObject
    {
        private int _speedLvl = 1;
        private int _power = 1;
        private int _maxBubble = 1;

        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }
    }
}