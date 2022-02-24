using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.IO;

namespace AliceMQTTHandler
{
    public class Settings
    {
        public bool Do_Output { get; set; }

        public int Web_Port { get; set; }
        public string Web_Path_Prefix { get; set; }

        public string MQTT_address { get; set; }
        public int MQTT_Port { get; set; }
        public string MQTT_Path { get; set; }

        public string MQTT_Username { get; set; }
        public string MQTT_Password { get; set; }

        public string Secret_Phrase { get; set; }

        public static bool Save(Settings settings,string path)
        {
            try
            {
                if (settings is null) return false;
                using(StreamWriter sw = new(path))
                {
                    sw.Write(JsonConvert.SerializeObject(settings));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static Settings Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                Settings result = null;
                using (StreamReader sr = new(path))
                {
                    string content = sr.ReadToEnd();
                    result = JsonConvert.DeserializeObject<Settings>(content);
                }
                return result;
            }
            catch(Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public static Settings Default()
        {
            return new()
            {
                Do_Output = true,
                Web_Port = 88,
                Web_Path_Prefix = "",
                MQTT_address = "localhost",
                MQTT_Port = 1883,
                MQTT_Path = "/",
                MQTT_Username = null,
                MQTT_Password = null,
                Secret_Phrase = GetRandomSecret()
            };

        }

        private static string GetRandomSecret(int length = 8)
        {
            Random rnd = new(DateTime.Now.Millisecond);
            int min = 'A';
            int max = 'z';
            string result = "";
            for (int i = 0; i < length; i++)
            {
                result += (char)rnd.Next(min, max+1);
            }
            return result;
        }
    }
}
