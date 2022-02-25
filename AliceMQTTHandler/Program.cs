using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;

namespace AliceMQTTHandler
{
    class Program
    {
        static Settings sets = Settings.Default();
        static string setting_path = "settings.json";
        static MqttClient mqclient = null;
        static WebServer server;
        static LampStatus lamp = new();
        static void Main(string[] args)
        {
            try
            {
                Settings s = Settings.Load(setting_path);
                if(s is null)
                {
                    Logger.Message("Config file was not found! Creating one");
                    Settings.Save(sets, setting_path);
                    return;
                }
                sets = s;
                server = new(sets.Web_Port, sets.Web_Path_Prefix, true);
                server.OnClientAccepted += WebClientAccepted;
                Logger.ApplySettings(sets);
                Logger.Message($"Webserver was loaded at {sets.Web_Port} port.");
                mqclient = new(sets.MQTT_address);
                string clientId = "AliceMQTTHandler-" + Guid.NewGuid().ToString();
                if (sets.MQTT_Username is not null && sets.MQTT_Password is not null)
                {
                    mqclient.Connect(clientId, sets.MQTT_Username, sets.MQTT_Password);
                }
                else
                {
                    mqclient.Connect(clientId);
                }
                mqclient.MqttMsgPublishReceived += (s,e)=>
                {
                    string message = Encoding.UTF8.GetString(e.Message);
                    string topic = e.Topic;
                    bool isnew = false;
                    if (topic.EndsWith("stt/PS"))
                    {
                        lamp.IsOn = Boolean.Parse(message);
                        isnew = true;
                    }
                    if (topic.EndsWith("stt/CL"))
                    {
                        int r, g, b;
                        r = Convert.ToInt32(message.Split(',')[0]);
                        g = Convert.ToInt32(message.Split(',')[1]);
                        b = Convert.ToInt32(message.Split(',')[2]);
                        lamp.Color = System.Drawing.Color.FromArgb(r, g, b);
                        isnew = true;
                    }
                    if (topic.EndsWith("stt/BR"))
                    {

                        lamp.Brightness = Convert.ToUInt16(message);
                        isnew = true;
                    }

                    if (topic.EndsWith("stt/EF"))
                    {
                        lamp.Effect = Convert.ToInt32(message);
                        isnew = true;
                    }
                    if(isnew)Logger.Message($"New matrix state: {lamp}");
                };
                string target_topic = CombinePath(sets.MQTT_Path, "#");
                mqclient.Subscribe(new[] { target_topic }, new byte[] { uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                Logger.Message($"MQTT client subscribed to \"{target_topic}\".");
                Logger.Message($"MQTT client connected to {sets.MQTT_address} with \"{clientId}\" id.");
                while (true)
                {
                    Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        static string WebClientAccepted(string method, string url, Dictionary<string,string> args)
        {
            if (url.StartsWith(sets.Web_Path_Prefix))
            {
                if(!args.ContainsKey("secret") || args["secret"] != sets.Secret_Phrase)
                {
                    Logger.Warn($"Attempted unauthorized access to {url}");
                }

                string result = null;
                string s_url = url.Replace(sets.Web_Path_Prefix, "");
                if (s_url[0] == '/') s_url = s_url[1..];

                if (s_url.StartsWith("setcolor"))
                {
                    string color = s_url.Replace("setcolor/", "");
                    int r, g, b;
                    r = Convert.ToInt32(color.Split('.')[0]);
                    g = Convert.ToInt32(color.Split('.')[1]);
                    b = Convert.ToInt32(color.Split('.')[2]);
                    System.Drawing.Color c = System.Drawing.Color.FromArgb(r, g, b);
                    mqclient.Publish(CombinePath(sets.MQTT_Path, "cmd"), Encoding.UTF8.GetBytes($"$5 0 {c.ToHexString().Replace("#","")};$5 2;"), uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                }
                if (s_url.StartsWith("getcolor"))
                {
                    JObject json = new();
                    string txt = lamp.Color.ToHexString().Replace("#", "");
                    json.Add("value",Convert.ToInt32(txt,16));
                    result = json.ToString();
                }

                if (s_url.StartsWith("setstate"))
                {
                    string state = s_url.Replace("setstate/", "");
                    mqclient.Publish(CombinePath(sets.MQTT_Path, "cmd"), Encoding.UTF8.GetBytes($"1 {state};"), uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                }
                if (s_url.StartsWith("getstate"))
                {
                    JObject json = new();
                    json.Add("value",lamp.IsOn);
                    result = json.ToString();
                }

                if (s_url.StartsWith("setbrightness"))
                {
                    string state = s_url.Replace("setbrightness/", "");
                    LampStatus lmp = new();
                    lmp.BrightnessInPercent = Convert.ToInt32(state);
                    mqclient.Publish(CombinePath(sets.MQTT_Path, "cmd"), Encoding.UTF8.GetBytes($"4 0 {lmp.Brightness};"), uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                }
                if (s_url.StartsWith("getbrightness"))
                {
                    JObject json = new();
                    json.Add("value", lamp.BrightnessInPercent);
                    result = json.ToString();
                }

                if (s_url.StartsWith("seteffect"))
                {
                    string state = s_url.Replace("seteffect/", "");
                    mqclient.Publish(CombinePath(sets.MQTT_Path, "cmd"), Encoding.UTF8.GetBytes($"8 0 {state};$16 1;$16 5 1;"), uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                }
                if (s_url.StartsWith("geteffect"))
                {
                    JObject json = new();
                    int ef = lamp.Effect;
                    if (ef > 42) ef = 42;
                    json.Add("value", ef);
                    result = json.ToString();
                }

                Logger.Message($"Request accepted at {url}");
                return result;
            }
            else
            {
                return null;
            }
        }
        static string CombinePath(string path1,string path2)
        {
            string result = path1;
            if (result.Length > 0 && result[result.Length - 1] != '/') result += '/';
            //else if (result.Length == 0) result = "/";
            if (path2.Length > 1 && path2[0] == '/') result += path2[1..];
            else result += path2;

            return result;
        }
    }
}
