using System;
using System.IO;

namespace Server.Data
{
    /*
     *  직렬화는 객체의 필드에 저장된 값을 메모리에 저장 가능하도록 바꾸는 것을 말합니다.
     *  (0과 1인 이진 형식, JSON, XML, 등 텍스트 형식도 가능)
     *  Serializable 애트리뷰트를 선언하면 이 형식(클래스)은 메모리에 저장 가능한 형식이 됩니다.
    */

    [Serializable]
    public class ServerConfig
    {
        public string dataPath;
        public string connectionString;
    }

    public class ConfigManager
    {
        public static ServerConfig Config { get; private set; }

        public static void LoadConfig()
        {
            string text = File.ReadAllText("config.json");
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfig>(text);
        }
    }
}